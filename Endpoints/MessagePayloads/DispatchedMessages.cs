using Microsoft.Azure.Functions.Worker;
using OLab.Common.Interfaces;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;
public class DispatchedMessages
{
  private readonly IOLabLogger _logger;

  public IList<object> Messages { get; set; }

  public DispatchedMessages(IOLabLogger logger)
  {
    Messages = new List<object>();
    _logger = logger;
  }

  /// <summary>
  /// Adds a signalr message to the queue
  /// </summary>
  /// <param name="message">Message to add</param>
  public void EnqueueMessage(TTalkMethod method)
  {
    var action = method.MessageAction();
    Messages.Add(action);

    _logger.LogInformation($"EnqueueMessage '{action.Target}' -> {method.Destination}: {{ {method} }}");
  }

  /// <summary>
  /// Adds a signalr add-to-group action to the queue
  /// </summary>
  /// <param name="message">Message to add</param>
  public void EnqueueAddConnectionToGroupAction(string connectionId, string groupName)
  {
    var action = new SignalRGroupAction(SignalRGroupActionType.Add)
    {
      GroupName = groupName,
      ConnectionId = connectionId
    };

    Messages.Add(action);

    _logger.LogInformation($"EnqueueAddConnectionToGroupAction '{connectionId}' -> '{groupName}'");
  }
}
