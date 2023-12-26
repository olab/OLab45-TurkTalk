using Microsoft.Azure.Functions.Worker;
using OLab.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;
public class DispatchedMessages
{
  private readonly IOLabLogger _logger;

  [SignalROutput(HubName = "Hub")]
  public IList<string> Messages { get; set; }

  public DispatchedMessages(IOLabLogger logger)
  {
    Messages = new List<string>();
    _logger = logger;
  }

  /// <summary>
  /// Adds a signalr message to the queue
  /// </summary>
  /// <param name="message">Message to add</param>
  public void EnqueueMessage(TTalkMethod method)
  {
    var action = method.MessageAction();
    Messages.Add(JsonSerializer.Serialize(action));

    _logger.LogInformation($"enqueued message '{action.Target}': {{ {method} }}");
  }

  /// <summary>
  /// Adds a signalr add-to-group action to the queue
  /// </summary>
  /// <param name="message">Message to add</param>
  public void EnqueueAddToGroupAction(string connectionId, string groupName)
  {
    var action = new SignalRGroupAction(SignalRGroupActionType.Add)
    {
      GroupName = groupName,
      ConnectionId = connectionId
    };

    Messages.Add(JsonSerializer.Serialize(action));

    _logger.LogInformation($"enqueuing SignalRGroupAction '{connectionId}' to group {groupName}");
  }
}
