using Dawn;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OLab.Access.Interfaces;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Endpoints.Utils;
using System.Configuration;
using System.Security.Claims;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public abstract class TTalkMethod
{
  public string GroupName { get; }
  public string ConnectionId { get; }

  protected readonly IOLabConfiguration Configuration;
  protected readonly string MethodName;
  public abstract object Arguments();

  public TTalkMethod(
    IOLabConfiguration configuration,
    string id,
    string methodName)
  {
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(id, nameof(id)).NotEmpty();
    Guard.Argument(methodName, nameof(methodName)).NotEmpty();

    Configuration = configuration;
    if (id.Contains("//"))
      GroupName = id;
    else
      ConnectionId = id;

    MethodName = methodName;
  }

  public SignalRMessageAction MessageAction()
  {
    var actionArguments = new object[] { Arguments() };

    if (!string.IsNullOrEmpty(GroupName))
      return new SignalRMessageAction(MethodName)
      {
        Arguments = actionArguments,
        GroupName = GroupName
      };

    if (!string.IsNullOrEmpty(ConnectionId))
      return new SignalRMessageAction(MethodName)
      {
        Arguments = actionArguments,
        ConnectionId = ConnectionId
      };

    throw new ArgumentNullException("Missing target id type for method");
  }
}