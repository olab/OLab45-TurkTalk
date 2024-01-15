using OLab.TurkTalk.Data.Contracts;

namespace OLab.FunctionApp.Functions.SignalR;

public class SendMessageRequest : RequestBase
{
  public uint TopicId{ get; set; }
  /// <summary>
  /// Target learner session id
  /// </summary>
  public string SessionId{ get; set; }
  /// <summary>
  /// Session that sent the message
  /// </summary>
  public string FromSessionId{ get; set; }
  /// <summary>
  /// Text message
  /// </summary>
  public string Message { set; get; }
}