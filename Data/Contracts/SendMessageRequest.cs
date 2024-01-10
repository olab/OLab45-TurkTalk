using OLab.TurkTalk.Data.Contracts;

namespace OLab.FunctionApp.Functions.SignalR;

public class SendMessageRequest : RequestBase
{
  public uint TopicId{ get; set; }
  public string SessionId{ get; set; }
  public string Message { set; get; }
}