using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace OLab.TurkTalk.Endpoints.Utils;

public class SignalRMessages
{
    [SignalROutput(HubName = "Hub")]
    public IEnumerable<SignalRMessageAction> Messages { get; set; }
}
