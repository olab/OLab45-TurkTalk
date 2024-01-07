using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.Mappers;
using OLab.TurkTalk.Data.Repositories;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using OLab.Common.Interfaces;
using Dawn;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;
public class TopicRoom
{
  private readonly DatabaseUnitOfWork DbUnitOfWork;

  public string Name { get; set; }
  public uint Id { get; set; }
  public uint TopicId { get; set; }
  public uint? ModeratorId { get; set; }
  public virtual ICollection<Attendee> Attendees { get; } = new List<Attendee>();
  public IOLabLogger Logger { get; }
  public virtual ConferenceTopic Topic { get; set; }
  public virtual TopicParticipant Moderator { get; set; }

  /// <summary>
  /// Group for room moderator commands (e.g. learner connect/disconnects)
  /// </summary>
  public string RoomModeratorChannel { get { return $"{TopicId}//{Id}//moderators"; } }

  /// <summary>
  /// Group for room learner commands (e.g. moderator connect/disconnects)
  /// </summary>
  public string RoomLearnersChannel { get { return $"{Topic.Id}//{Id}//learners"; } }

  public TopicRoom()
  {

  }

  public TopicRoom(
    IOLabLogger logger,
    ConferenceTopic topic,
    DatabaseUnitOfWork dbUnitOfWork)
  {
    Logger = logger;
    Topic = topic;
    DbUnitOfWork = dbUnitOfWork;
  }

  /// <summary>
  /// Assign a moderator to a room
  /// </summary>
  /// <param name="dbUnitOfWork"></param>
  /// <param name="dtoModerator"></param>
  /// <param name="messageQueue"></param>
  /// <returns></returns>
  public async Task<TopicParticipant> AssignModeratorToRoom(
    DatabaseUnitOfWork dbUnitOfWork,
    TopicParticipant dtoModerator,
    DispatchedMessages messageQueue)
  {
    var physModerator = new TtalkTopicParticipant
    {
      SessionId = dtoModerator.SessionId,
      UserId = dtoModerator.UserId,
      UserName = dtoModerator.UserName,
      TokenIssuer = dtoModerator.TokenIssuer,
      RoomId = Id,
      ConnectionId = dtoModerator?.ConnectionId,
    };

    await dbUnitOfWork.TopicParticipantRepository.InsertAsync(physModerator);
    dbUnitOfWork.Save();

    var mapper = new TopicParticipantMapper(Topic.Conference.Logger);
    dtoModerator = mapper.PhysicalToDto(physModerator);

    Moderator = dtoModerator;

    // update the moderator in the database
    var physRoom = dbUnitOfWork
      .TopicRoomRepository
      .Get(x => x.Id == Id).FirstOrDefault();

    physRoom.ModeratorId = Moderator.Id;
    dbUnitOfWork
      .TopicRoomRepository
      .Update(physRoom);

    // create and add connection to room moderator channel
    messageQueue.EnqueueAddConnectionToGroupAction(
      dtoModerator.ConnectionId,
      RoomModeratorChannel);

    // signal moderator added to new moderated room
    messageQueue.EnqueueMessage(new RoomAcceptedMethod(
        Topic.Conference.Configuration,
        RoomModeratorChannel,
        Name,
        Id,
        0,
        Moderator.NickName,
        true));

    return dtoModerator;
  }

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
      RoomModeratorChannel);

    // signal moderator added to new moderated room
    messageQueue.EnqueueMessage(new RoomAcceptedMethod(
        Topic.Conference.Configuration,
        RoomModeratorChannel,
        physRoom.Topic.Name,
        physRoom.Id,
        0,
        physModerator.NickName,
        true));
  }

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
    DbUnitOfWork.Save();

    // update the moderator with the room id
    physModerator.RoomId = phys.Id;
    DbUnitOfWork
      .TopicParticipantRepository.Update(physModerator);

    // explicit save needed because we need new inserted Id 
    DbUnitOfWork.Save();

    phys.Topic = topic;

    Logger.LogInformation($"created topic '{topic.Name}' room ({phys.Id})");

    return phys;
  }

  internal async Task<TtalkTopicRoom> GetAsync(uint topicId)
  {
    return await DbUnitOfWork.TopicRoomRepository.GetByIdAsync(topicId);
  }
}
