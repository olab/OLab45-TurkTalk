using OLab.Common.Interfaces;
using OLab.FunctionApp.Functions.SignalR;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;
internal class MessageMethod : TTalkMethod
{
  public uint TopicId{ get; set; }
  /// <summary>
  /// Session that receives the message
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
  public bool IsSystemMessage { get; set; }

  public MessageMethod(
    IOLabConfiguration configuration,
    SendMessageRequest payload,
    bool isSystemMessage = false) : base(configuration, "", "message")
  {
    TopicId = payload.TopicId;
    SessionId = payload.SessionId;
    FromSessionId = payload.FromSessionId;
    Message = payload.Message;
    IsSystemMessage = isSystemMessage;

    GroupName = $"{payload.TopicId}//{payload.SessionId}//session";
  }

  public override object Arguments()
  {
    return this;
  }
}
