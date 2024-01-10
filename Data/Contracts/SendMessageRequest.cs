using OLab.TurkTalk.Data.Contracts;

namespace OLab.FunctionApp.Functions.SignalR;

public class SendMessageRequest : RequestBase
{
  public string ToGroupName{ get; set; }
}