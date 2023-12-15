using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;
public class TTalkDispatchedMessages
{
  [SignalROutput(HubName = "Hub")]
  public IEnumerable<string> Messages { get; set; }
}
