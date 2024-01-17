using Dawn;
using OLab.Common.Interfaces;
using OLab.FunctionApp.Functions.SignalR;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Data.Repositories;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;
public class TopicRoomHelper
{
  private readonly DatabaseUnitOfWork _dbUnitOfWork;
  private readonly IOLabLogger _logger;
  private ConferenceTopicHelper _topicHelper { get; set; }

  public TopicRoomHelper()
  {
  }

  public TopicRoomHelper(
    IOLabLogger logger,
    ConferenceTopicHelper topicHelper,
    DatabaseUnitOfWork dbUnitOfWork)
  {
    Guard.Argument(logger, nameof(logger)).NotNull();
    Guard.Argument(topicHelper, nameof(topicHelper)).NotNull();
    Guard.Argument(dbUnitOfWork, nameof(dbUnitOfWork)).NotNull();

    _logger = logger;
    _topicHelper = topicHelper;
    _dbUnitOfWork = dbUnitOfWork;
  }

  /// <summary>
  /// Register moderator with SignalR and notify
  /// </summary>
  /// <param name="messageQueue">Dispatch messages</param>
  /// <param name="physRoom">Room</param>
  /// <param name="physModerator">Moderator</param>
  internal void RegisterModerator(
    DispatchedMessages messageQueue,
    TtalkTopicRoom physRoom,
    TtalkTopicParticipant physModerator)
  {
    Guard.Argument(messageQueue, nameof(messageQueue)).NotNull();
    Guard.Argument(physRoom, nameof(physRoom)).NotNull();
    Guard.Argument(physModerator, nameof(physModerator)).NotNull();

    // create and add connection to room moderator channel
    messageQueue.EnqueueAddConnectionToGroupAction(
      physModerator.ConnectionId,
      physRoom.RoomModeratorChannel);

    // signal moderator added to new moderated room
    messageQueue.EnqueueMessage(new RoomAcceptedMethod(
        _topicHelper.Conference.Configuration,
        physRoom.RoomModeratorChannel,
        physRoom,
        0,
        physModerator,
        true));
  }

  /// <summary>
  /// Register learner with SignalR and notify
  /// </summary>
  /// <param name="messageQueue">Dispatch messages</param>
  /// <param name="physRoom">Room</param>
  /// <param name="physLearner">Learner assigned</param>
  /// <param name="physModerator">Moderator</param>
  internal void RegisterLearner(
    DispatchedMessages messageQueue,
    TtalkTopicRoom physRoom,
    TtalkTopicParticipant physLearner,
    TtalkTopicParticipant physModerator,
    uint seatNumber)
  {
    Guard.Argument(messageQueue, nameof(messageQueue)).NotNull();
    Guard.Argument(physRoom, nameof(physRoom)).NotNull();
    Guard.Argument(physLearner, nameof(physLearner)).NotNull();
    Guard.Argument(physModerator, nameof(physModerator)).NotNull();

    // signal to learner added to room
    messageQueue.EnqueueMessage(new RoomAcceptedMethod(
        _topicHelper.Conference.Configuration,
        physLearner.RoomLearnerSessionChannel,
        physRoom,
        seatNumber,
        physModerator,
        true));

    // signal to moderator learner (re)added
    messageQueue.EnqueueMessage(new LearnerAssignedMethod(
        _topicHelper.Conference.Configuration,
        physRoom.RoomModeratorChannel,
        physRoom,
        seatNumber,
        physLearner,
        true));
  }

  /// <summary>
  /// Create topic room
  /// </summary>
  /// <param name="topic">Parent topic</param>
  /// <param name="physModerator">moderator</param>
  /// <returns></returns>
  internal async Task<TtalkTopicRoom> CreateRoomAsync(
    TtalkConferenceTopic topic,
    TtalkTopicParticipant physModerator)
  {
    var phys = new TtalkTopicRoom
    {
      TopicId = topic.Id,
      ModeratorId = physModerator.Id
    };

    await _dbUnitOfWork
      .TopicRoomRepository
      .InsertAsync(phys);

    // explicit save needed because we need new inserted Id 
    _dbUnitOfWork.Save();

    // update the moderator with the room id
    physModerator.RoomId = phys.Id;
    physModerator.TopicId = topic.Id;

    _dbUnitOfWork
      .TopicParticipantRepository.Update(physModerator);

    // explicit save needed because we need new inserted Id 
    _dbUnitOfWork.Save();

    _logger.LogInformation($"created topic room '{topic.Name}' ({phys.Id}). moderator id {physModerator.Id}");

    phys.Topic = topic;


    return phys;
  }

  internal TtalkTopicRoom Get(uint id)
  {
    var phys = _dbUnitOfWork
      .TopicRoomRepository
      .Get(
        filter: x => x.Id == id,
        includeProperties: "Topic"
      )
      .FirstOrDefault();

    if (phys == null)
      _logger.LogWarning($"topic room id {id} does not exist");

    return phys;
  }

  /// <summary>
  /// Moderator requests assign learner to room
  /// </summary>
  /// <param name="messageQueue">Dispatched messages</param>
  /// <param name="learnerSessionId">learner session id</param>
  /// <param name="moderatorSessionId">requesting moderator</param>
  /// <param name="seatNumber">optional seat number</param>
  internal async Task AssignLearnerToRoomAsync(
    DispatchedMessages messageQueue,
    string learnerSessionId,
    string moderatorSessionId,
    uint? seatNumber)
  {
    TtalkTopicRoom physRoom = null;

    var physModerator = _dbUnitOfWork
      .TopicParticipantRepository
      .GetModeratorBySessionId(moderatorSessionId);

    physRoom =
      Get(physModerator.RoomId.Value);

    var atriumLearners = _topicHelper.GetTopicParticipants(physModerator.TopicId.Value);

    // get and update the room assignment for learner
    var physLearner =
      atriumLearners.FirstOrDefault(x => x.SessionId == learnerSessionId);

    // assign seat, if none specified
    seatNumber = _topicHelper.GetSeatNumber(atriumLearners, seatNumber);

    _dbUnitOfWork
      .TopicParticipantRepository
      .AssignToRoom(
        learnerSessionId,
        physModerator.RoomId.Value,
        seatNumber.Value);

    _dbUnitOfWork.Save();

    RegisterLearner(
      messageQueue,
      physRoom,
      physLearner,
      physModerator,
      seatNumber.Value);

    _logger.LogInformation($"learner '{physLearner.NickName}' ({physLearner.Id}). assigned to room {physRoom.Id}, seat {seatNumber}");

    // notify moderators of atrium change
    await _topicHelper.SignalAtriumChangeAsync(
      physRoom.Topic,
      messageQueue);
  }

  /// <summary>
  /// Send message to group
  /// </summary>
  /// <param name="payload"></param>
  internal void SendMessage(
    SendMessageRequest payload,
    DispatchedMessages messageQueue)
  {
    // signal message to learner group
    messageQueue.EnqueueMessage(new MessageMethod(
      _topicHelper.Conference.Configuration,
      payload));
  }
}
