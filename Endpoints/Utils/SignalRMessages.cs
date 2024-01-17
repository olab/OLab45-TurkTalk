using Microsoft.Azure.Functions.Worker;

namespace OLab.TurkTalk.Endpoints.Utils;

public class SignalRMessages
{
  [SignalROutput(HubName = "Hub")]
  public IEnumerable<SignalRMessageAction> Messages { get; set; }
}
