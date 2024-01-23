using Dawn;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Models;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class RoomAcceptedMethod : TTalkMethod
{
  //  payload properties
  public uint RoomId { get; set; }
  public uint TopicId { get; set; }
  public string RoomName { get; set; }
  public uint SeatNumber { get; }
  public TopicParticipantDto Moderator { get; set; }
  public bool WasAdded { get; set; }

  public RoomAcceptedMethod(
    IOLabConfiguration configuration,
    string groupName,
    TtalkTopicRoom physRoom,
    uint seatNumber,
    TtalkTopicParticipant physModerator,
    bool wasAdded) : base(
      configuration,
      groupName,
      "roomaccepted")
  {
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(groupName, nameof(groupName)).NotEmpty();
    Guard.Argument(physRoom, nameof(physRoom)).NotNull();
    Guard.Argument(physModerator, nameof(physModerator)).NotNull();

    RoomId = physRoom.Id;
    TopicId = physRoom.Topic.Id;
    RoomName = physRoom.Topic.Name;
    SeatNumber = seatNumber;
    Moderator = new TopicParticipantDto( physModerator );
    WasAdded = wasAdded;
  }

  public override object Arguments()
  {
    return this;
  }

  public override string ToString()
  {
    return $"room {RoomName} moderator {Moderator.UserName} -> {Destination}";
  }
}
