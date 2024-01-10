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
  private readonly SendMessageRequest payload;

  public MessageMethod(
    IOLabConfiguration configuration,
    SendMessageRequest payload) : base(configuration, "", "sendmessage")
  {
    this.payload = payload;
    GroupName = $"{payload.TopicId}//{payload.SessionId}//session";
  }

  public override object Arguments()
  {
    return this;
  }
}
