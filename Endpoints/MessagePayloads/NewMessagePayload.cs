using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class NewMessagePayload
{
  public string ConnectionId { get; }
  public string Sender { get; }
  public string Text { get; }

  public NewMessagePayload(SignalRInvocationContext invocationContext, string message)
  {
    Sender = string.IsNullOrEmpty(invocationContext.UserId) ? string.Empty : invocationContext.UserId;
    ConnectionId = invocationContext.ConnectionId;
    Text = message;
  }
}
