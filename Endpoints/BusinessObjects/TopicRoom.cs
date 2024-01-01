using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.Mappers;
using OLab.TurkTalk.Data.Repositories;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;
public class TopicRoom
{
  public string Name { get; set; }
  public uint Id { get; set; }
  public uint TopicId { get; set; }
  public uint? ModeratorId { get; set; }
  public virtual ICollection<Attendee> Attendees { get; } = new List<Attendee>();
  public virtual ConferenceTopic Topic { get; set; }
  public virtual TopicParticipant Moderator { get; set; }

  public string RoomModeratorChannel { get { return $"{TopicId}//{Id}//moderator"; } }

  public string RoomLearnersChannel { get { return $"{Topic.Id}//{Id}//learners"; } }

  public async Task<TopicParticipant> AssignModerator(
    DatabaseUnitOfWork dbUnitOfWork,
    TopicParticipant dtoModerator,
    DispatchedMessages messageQueue)
  {
    var physModerator = new TtalkTopicParticipant
    {
      SessionId = dtoModerator.SessionId,
      TopicId = Topic.Id,
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
        this,
        0,
        true));

    return dtoModerator;
  }
}
