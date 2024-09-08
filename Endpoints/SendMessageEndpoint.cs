using OLab.FunctionApp.Functions.SignalR;

namespace OLab.TurkTalk.Endpoints;

public partial class TurkTalkEndpoint
{
  public void SendMessage(
    SendMessageRequest payload)
  {
    RoomHelper.SendMessage(payload, MessageQueue);
  }
}
