using Dawn;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Models;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class LearnerAssignedMethod : TTalkMethod
{
  //  payload properties
  public uint RoomId { get; set; }
  public uint TopicId { get; set; }
  public string RoomName { get; set; }
  public uint SeatNumber { get; }
  public bool WasAdded { get; set; }
  public TopicParticipantDto DtoLearner { get; set; }

  public LearnerAssignedMethod(
    IOLabConfiguration configuration,
    string groupName,
    TtalkTopicRoom physRoom,
    uint seatNumber,
    TtalkTopicParticipant physLearner,
    bool wasAdded) : base(
      configuration,
      groupName,
      "learnerassigned")
  {
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(groupName, nameof(groupName)).NotEmpty();
    Guard.Argument(physRoom, nameof(physRoom)).NotNull();
    Guard.Argument(physLearner, nameof(physLearner)).NotNull();

    RoomId = physRoom.Id;
    TopicId = physRoom.Topic.Id;
    RoomName = physRoom.Topic.Name;
    SeatNumber = seatNumber;
    WasAdded = wasAdded;
    DtoLearner = new TopicParticipantDto(physLearner);
  }

  public override object Arguments()
  {
    return this;
  }

  public override string ToString()
  {
    return $"room {RoomName} -> {Destination}";
  }
}
