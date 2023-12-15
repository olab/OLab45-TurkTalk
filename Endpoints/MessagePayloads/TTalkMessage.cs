using DocumentFormat.OpenXml.Presentation;
using Microsoft.Azure.Functions.Worker;
using OLab.Access.Interfaces;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Endpoints.Utils;
using System.Configuration;
using System.Security.Claims;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public abstract class TTalkMessage
{
  public string ConnectionId { get; }

  protected readonly IOLabConfiguration Configuration;
  protected readonly string MethodName;
  public abstract object Arguments();

  public TTalkMessage(
    IOLabConfiguration configuration,
    string connectionId,
    string methodName)
  {
    Configuration = configuration;
    ConnectionId = connectionId;
    MethodName = methodName;
  }

  public SignalRMessageAction Message()
  {
    var actionArguments = new object[] { Arguments() };
    return new SignalRMessageAction(MethodName)
    {
      Arguments = actionArguments
    };
  }
}