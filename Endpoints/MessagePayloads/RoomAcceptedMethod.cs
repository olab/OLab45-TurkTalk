using Dawn;
using DocumentFormat.OpenXml.Spreadsheet;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Endpoints.BusinessObjects;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class RoomAcceptedMethod: TTalkMethod
{
  //  payload properties
  public string RoomName { get; set; }
  public uint SeatNumber { get; }
  public string ModeratorName { get; set; }
  public bool WasAdded { get; set; }

  public RoomAcceptedMethod(
    IOLabConfiguration configuration,
    string groupName,
    TopicRoom room,
    uint seatNumber,
    bool wasAdded) : base(
      configuration,
      groupName,
      "roomaccepted")
  {
    Guard.Argument(room).NotNull(nameof(room));

    RoomName = room.Name;
    SeatNumber = seatNumber;
    ModeratorName = room.Moderator.UserName;
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
