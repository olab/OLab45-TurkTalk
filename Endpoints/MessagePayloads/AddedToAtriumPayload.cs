using OLab.Access.Interfaces;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Endpoints.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class AddedToAtriumPayload
{
  public string Command { get; }
  public string ConnectionId { get; }

  public AddedToAtriumPayload(
    IOLabConfiguration configuration,
    string connectionId)
  {
    ConnectionId = connectionId;
    Command = "ATRIUMADD";
  }
}
