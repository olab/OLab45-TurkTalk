using Dawn;
using OLab.Common.Interfaces;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class RoomAcceptedMethod: TTalkMethod
{
  public string RoomName { get; set; }
  public string ModeratorName { get; set; }

  public RoomAcceptedMethod(
    IOLabConfiguration configuration,
    string connectionId,
    string roomName,
    string moderatorName) : base(
      configuration,
      connectionId,
      "roomaccepted")
  {
    Guard.Argument(roomName, nameof(roomName)).NotEmpty();
    Guard.Argument(moderatorName, nameof(moderatorName)).NotEmpty();

    RoomName = roomName;
    ModeratorName = moderatorName;
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
