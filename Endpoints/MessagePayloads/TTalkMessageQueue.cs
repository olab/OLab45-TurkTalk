using Microsoft.Azure.Functions.Worker;
using OLab.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;
public class TTalkMessageQueue
{
  private readonly IOLabLogger _logger;

  [SignalROutput(HubName = "Hub")]
  public IList<SignalRMessageAction> MessageActions { get; set; }

  public TTalkMessageQueue(IOLabLogger logger)
  {
    MessageActions = new List<SignalRMessageAction>();
    _logger = logger;
  }

  /// <summary>
  /// Adds a signalr message to the queue
  /// </summary>
  /// <param name="message">Message to add</param>
  public void EnqueueMethod(TTalkMethod method)
  {
    var action = method.MessageAction();
    MessageActions.Add(action);

    _logger.LogInformation($"enqueuing message '{action.Target}': {{ {method} }}");

  }
}
