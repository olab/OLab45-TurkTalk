using Dawn;
using OLab.Common.Interfaces;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class AtriumAcceptedMethod: TTalkMethod
{
  public string RoomName { get; set; }

  public AtriumAcceptedMethod(
    IOLabConfiguration configuration,
    string connectionId,
    string roomName) : base(
      configuration,
      connectionId,
      "atriumaccepted")
  {
    Guard.Argument(roomName, nameof(roomName)).NotEmpty();
    RoomName = roomName;
  }

  public override object Arguments()
  {
    return this;
  }
}
