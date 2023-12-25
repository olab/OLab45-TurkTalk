using Dawn;
using DocumentFormat.OpenXml.Spreadsheet;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Endpoints.BusinessObjects;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class RoomAcceptedMethod: TTalkMethod
{
  public string RoomName { get; set; }
  public uint SeatNumber { get; }
  public string ModeratorName { get; set; }
  public bool WasAdded { get; set; }

  public RoomAcceptedMethod(
    IOLabConfiguration configuration,
    string connectionId,
    TopicRoom room,
    uint seatNumber,
    bool wasAdded) : base(
      configuration,
      connectionId,
      "roomaccepted")
  {
    Guard.Argument(room).NotNull(nameof(room));
    Guard.Argument(connectionId, nameof(connectionId)).NotEmpty();

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
    return $"{RoomName} {ModeratorName}";
  }
}
