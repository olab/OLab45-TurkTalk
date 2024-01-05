using Dawn;
using OLab.Common.Interfaces;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class RoomAcceptedMethod : TTalkMethod
{
  //  payload properties
  public uint RoomId { get; set; }
  public string RoomName { get; set; }
  public uint SeatNumber { get; }
  public string ModeratorName { get; set; }
  public bool WasAdded { get; set; }

  public RoomAcceptedMethod(
    IOLabConfiguration configuration,
    string channelName,
    string roomName,
    uint roomId,
    uint seatNumber,
    string moderatorName,
    bool wasAdded) : base(
      configuration,
      channelName,
      "roomaccepted")
  {
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(channelName).NotEmpty(nameof(channelName));
    Guard.Argument(roomName).NotEmpty(nameof(roomName));
    Guard.Argument(roomId, nameof(roomId)).NotZero();
    Guard.Argument(moderatorName).NotEmpty(nameof(moderatorName));

    RoomName = roomName;
    SeatNumber = seatNumber;
    ModeratorName = moderatorName;
    WasAdded = wasAdded;
  }

  public override object Arguments()
  {
    return this;
  }

  public override string ToString()
  {
    return $"room {RoomName} moderator {ModeratorName} -> {Destination}";
  }
}
