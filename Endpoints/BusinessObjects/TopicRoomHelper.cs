using Dawn;
using OLab.Common.Interfaces;
using OLab.FunctionApp.Functions.SignalR;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Data.Repositories;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using System.Diagnostics.CodeAnalysis;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;
public class TopicRoomHelper : OLabHelper
{
  private ConferenceTopicHelper _topicHelper { get; set; }

  public TopicRoomHelper(
    IOLabLogger logger,
    ConferenceTopicHelper topicHelper,
    DatabaseUnitOfWork dbUnitOfWork) : base(logger, dbUnitOfWork)
  {
    Guard.Argument(topicHelper, nameof(topicHelper)).NotNull();

    _topicHelper = topicHelper;
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
  /// Signals new learner with SignalR
  /// </summary>
  /// <param name="messageQueue">Dispatch messages</param>
  /// <param name="physRoom">Room</param>
  /// <param name="physLearner">Learner assigned</param>
  /// <param name="physModerator">Moderator</param>
  internal void BroadcastNewLearner(
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

    Logger.LogInformation($"learner '{physLearner.NickName}' ({physLearner.Id}). assigned to room {physRoom.Id}, seat {seatNumber}");

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

    await DbUnitOfWork
      .TopicRoomRepository
      .InsertAsync(phys);

    // explicit save needed because we need new inserted Id 
    CommitChanges();

    // update the moderator with the room id
    physModerator.RoomId = phys.Id;
    physModerator.TopicId = topic.Id;

    DbUnitOfWork
      .TopicParticipantRepository.Update(physModerator);

    // explicit save needed because we need new inserted Id 
    CommitChanges();

    Logger.LogInformation($"created topic room '{topic.Name}' ({phys.Id}). moderator id {physModerator.Id}");

    phys.Topic = topic;


    return phys;
  }

  /// <summary>
  /// Get room
  /// </summary>
  /// <param name="id">Room id</param>
  /// <param name="allowNull">throw exception if not found</param>
  /// <returns>TtalkTopicRoom</returns>
  /// <exception cref="Exception">Room id not found</exception>
  internal TtalkTopicRoom Get(uint? id, bool allowNull = true)
  {
    TtalkTopicRoom phys = null;

    if (id.HasValue)
      phys = DbUnitOfWork
        .TopicRoomRepository
        .Get(
          filter: x => x.Id == id,
          includeProperties: "Topic, Moderator"
        )
        .FirstOrDefault();

    if (phys == null)
      Logger.LogWarning($"topic room id {id} does not exist");

    if ((phys == null) && !allowNull)
      throw new Exception($"unable to find room for id '{id}'");

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
    var physModerator = DbUnitOfWork
      .TopicParticipantRepository
      .GetModeratorBySessionId(moderatorSessionId);

    var physRoom =
      Get(physModerator.RoomId.Value, false);

    _topicHelper
      .ParticipantHelper
      .LoadByTopicId(
      physModerator.TopicId.Value,
      physModerator.RoomId.Value);

    // get and update the room assignment for learner
    var physLearner =
      _topicHelper.ParticipantHelper.GetBySessionId(learnerSessionId, false);

    // assign seat, if none specified
    seatNumber = _topicHelper.GetSeatNumber(
      _topicHelper.ParticipantHelper.Participants,
      seatNumber);

    _topicHelper.ParticipantHelper.AssignLearnerToRoom(
        learnerSessionId,
        physModerator.RoomId.Value,
        seatNumber.Value);

    BroadcastNewLearner(
      messageQueue,
      physRoom,
      physLearner,
      physModerator,
      seatNumber.Value);

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

  /// <summary>
  /// Signal room that learner has disconnected
  /// </summary>
  /// <param name="messageQueue">Dispatched messages</param>
  /// <param name="physRoom">Target room</param>
  /// <param name="physLearner">Learner to remove</param>
  internal void DisconnectLearner(
    TtalkTopicRoom physRoom,
    TtalkTopicParticipant physLearner,
    DispatchedMessages messageQueue)
  {
    // signal to topic moderators disconnected learner
    messageQueue.EnqueueMessage(new LearnerStatusMethod(
      _topicHelper.Conference.Configuration,
      physRoom,
      physLearner,
      false));
  }

  /// <summary>
  /// Signal room that learner has disconnected
  /// </summary>
  /// <param name="messageQueue">Dispatched messages</param>
  /// <param name="physRoom">Target room</param>
  /// <param name="physModerator">Moderator to remove</param>
  internal void DisconnectModerator(
    TtalkTopicRoom physRoom,
    TtalkTopicParticipant physModerator,
    DispatchedMessages messageQueue)
  {
    // signal to topic moderators disconnected learner
    messageQueue.EnqueueMessage(new ModeratorStatusMethod(
      _topicHelper.Conference.Configuration,
      physRoom,
      physModerator,
      false));
  }
}
