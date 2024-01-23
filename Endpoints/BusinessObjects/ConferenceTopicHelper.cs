using Dawn;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Data.Repositories;
using OLab.TurkTalk.Endpoints.Interface;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;
public class ConferenceTopicHelper : OLabHelper
{
  public IConference Conference;

  public readonly TopicRoomHelper RoomHelper;
  public readonly TopicParticipantHelper ParticipantHelper;
  private readonly SemaphoreSlim _topicSemaphore = new SemaphoreSlim(1, 1);
  private readonly SemaphoreSlim _atriumSemaphore = new SemaphoreSlim(1, 1);

  public ConferenceTopicHelper()
  {
  }

  public ConferenceTopicHelper(
    IOLabLogger logger,
    IConference conference,
    DatabaseUnitOfWork dbUnitOfWork) : base(logger, dbUnitOfWork)
  {
    Guard.Argument(conference).NotNull(nameof(conference));

    Conference = conference;

    RoomHelper = new TopicRoomHelper(
      Logger,
      this,
      dbUnitOfWork);

    ParticipantHelper = new TopicParticipantHelper(
      Logger,
      dbUnitOfWork);
  }

  /// <summary>
  /// Get/create topic
  /// </summary>
  /// <param name="conference">Owning conference</param>
  /// <param name="nodeId">Node id containing ttalk question (0 = root node)</param>
  /// <param name="questionId">Turktalk question id</param>
  /// <returns>Conference topic</returns>
  internal async Task<TtalkConferenceTopic> GetCreateTopicAsync(
    IConference conference,
    uint nodeId,
    uint questionId)
  {
    Guard.Argument(conference, nameof(conference)).NotNull();
    Guard.Argument(questionId, nameof(questionId)).Positive();
    Guard.Argument(nodeId, nameof(nodeId)).Positive();

    try
    {
      var topicName =
        GetTopicNameFromQuestion(questionId);

      await SemaphoreLogger.WaitAsync(
        Logger,
        $"topic '{nodeId}:{questionId}' creation",
        _topicSemaphore);

      var phys =
        await DbUnitOfWork
          .ConferenceTopicRepository
          .GetCreateTopicAsync(conference.Id, nodeId, topicName);

      // topic any topic participants into participant helper
      ParticipantHelper.LoadFromTopic(phys);

      return phys;

    }
    finally
    {
      SemaphoreLogger.Release(
        Logger,
        $"topic '{nodeId}:{questionId}' creation",
        _topicSemaphore);
    }
  }

  /// <summary>
  /// Gets a topic from the database
  /// </summary>
  /// <param name="topicId">Topic id</param>
  /// <param name="allowNull">throw exception if not found</param>
  /// <returns>Conference topic</returns>
  internal async Task<TtalkConferenceTopic> GetAsync(uint? id, bool allowNull = true)
  {
    TtalkConferenceTopic phys = null;

    if (id.HasValue)
      phys =
        await DbUnitOfWork
          .ConferenceTopicRepository
          .GetByIdAsync(id.Value);

    if (phys == null)
      Logger.LogWarning($"topic {id} does not exist");

    if ((phys == null) && !allowNull)
      throw new Exception($"unable to find topic for id '{id}'");

    return phys;

  }

  internal async Task RegisterModeratorAsync(
    DispatchedMessages messageQueue,
    TtalkConferenceTopic physTopic,
    TtalkTopicParticipant physModerator)
  {
    Guard.Argument(messageQueue, nameof(messageQueue)).NotNull();
    Guard.Argument(physTopic, nameof(physTopic)).NotNull();
    Guard.Argument(physModerator, nameof(physModerator)).NotNull();

    // create and add connection to topic moderators channel
    messageQueue.EnqueueAddConnectionToGroupAction(
      physModerator.ConnectionId,
      physTopic.TopicModeratorsChannel);

    // send current atrium contents
    await SignalAtriumChangeAsync(
      physTopic,
      messageQueue);
  }

  internal async Task SignalAtriumChangeAsync(
    TtalkConferenceTopic physTopic,
    DispatchedMessages messageQueue)
  {
    Guard.Argument(physTopic, nameof(physTopic)).NotNull();
    Guard.Argument(messageQueue, nameof(messageQueue)).NotNull();

    try
    {
      await SemaphoreLogger.WaitAsync(
        Logger,
        $"atrium {physTopic.Id}",
        _atriumSemaphore);

      // load atrium users for topic
      var atriumLearners = DbUnitOfWork
        .TopicParticipantRepository
        .GetAtriumLearnersForTopic(physTopic.Id).OrderBy(x => x.NickName)
        .ToList();

      // signal to topic moderators atrium update
      messageQueue.EnqueueMessage(new AtriumUpdateMethod(
        Conference.Configuration,
        physTopic.TopicModeratorsChannel,
        atriumLearners));
    }
    finally
    {
      SemaphoreLogger.Release(
        Logger,
        $"atrium {physTopic.Id}",
        _atriumSemaphore);
    }

  }

  internal async Task BroadcastAtriumAddition(
    TtalkConferenceTopic physTopic,
    TtalkTopicParticipant physLearner,
    int numberOfModerators,
    DispatchedMessages messageQueue)
  {
    Guard.Argument(physLearner, nameof(physLearner)).NotNull();
    Guard.Argument(messageQueue, nameof(messageQueue)).NotNull();
    Guard.Argument(physTopic, nameof(physTopic)).NotNull();

    Logger.LogInformation($"adding '{physLearner.NickName}' to atrium.");

    if (physLearner != null)
      // signal to learner 'new' add to atrium
      messageQueue.EnqueueMessage(new AtriumAcceptedMethod(
        Conference.Configuration,
        physLearner.RoomLearnerSessionChannel,
        physTopic.Name,
        numberOfModerators,
        true));

    // signal atrium change to topic
    await SignalAtriumChangeAsync(
      physTopic,
      messageQueue);
  }

  /// <summary>
  /// Gets open/requested seat number
  /// </summary>
  /// <param name="atriumLearners">List of participants</param>
  /// <param name="seatNumberPerference">Seat number preference</param>
  /// <returns>New seat number</returns>
  internal uint GetSeatNumber(IList<TtalkTopicParticipant> atriumLearners, uint? seatNumberPerference)
  {
    uint seatNumber = 0;

    if (seatNumberPerference.HasValue)
    {
      seatNumber = seatNumberPerference.Value;
      Logger.LogInformation($"using seat number perference {seatNumber}");
    }

    else
    {
      for (uint i = 1; i <= 8; i++)
      {
        var seat = atriumLearners.FirstOrDefault(x => x.SeatNumber.HasValue && x.SeatNumber.Value == i);
        if (seat == null)
        {
          seatNumber = i;
          Logger.LogInformation($"found open seat number {seatNumber}");
          break;
        }
      }
    }

    return seatNumber;
  }

  /// <summary>
  /// Gets the topic name from the TTalk question
  /// </summary>
  /// <param name="questionId">TTalk question id</param>
  /// <returns>Topic Name</returns>
  /// <exception cref="Exception">Could not find question id or question room/topic name</exception>
  public string GetTopicNameFromQuestion(uint questionId)
  {
    // ensure question is valid and is of correct type (ttalk)
    var question = DbUnitOfWork
      .DbContextOLab
      .SystemQuestions
      .FirstOrDefault(x => x.Id == questionId &&
        (x.EntryTypeId == 11 || x.EntryTypeId == 15)) ??
      throw new Exception($"question id {questionId} not found/invalid");

    var questionSetting =
      JsonConvert.DeserializeObject<QuestionSetting>(question.Settings);

    if (string.IsNullOrEmpty(questionSetting.RoomName))
      throw new Exception($"unable to get room name from question id {questionId}");

    return questionSetting.RoomName;
  }

  /// <summary>
  /// Get learner by session id
  /// </summary>
  /// <param name="sessionId">Session id</param>
  /// <returns>Participant (or null of not found)</returns>
  //internal TtalkTopicParticipant GetLearnerBySessionId(
  //  string sessionId,
  //  bool allowNull = true)
  //{
  //  var phys = DbUnitOfWork
  //    .TopicParticipantRepository
  //    .GetLearnerBySessionId(sessionId);

  //  if ((phys == null && !allowNull))
  //    throw new Exception($"learner session id {sessionId} not found");

  //  return phys;
  //}
}
