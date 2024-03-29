using Dawn;
using Microsoft.Azure.Functions.Worker;
using OLab.Common.Interfaces;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public abstract class TTalkMethod
{
  public string GroupName { get; protected set; }
  public string ConnectionId { get; }
  public string Destination
  {
    get
    {
      if (!string.IsNullOrEmpty(GroupName))
        return GroupName;
      if (!string.IsNullOrEmpty(ConnectionId))
        return ConnectionId;
      return "????";
    }
  }

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
    SignalRMessageAction action = null;

    if (!string.IsNullOrEmpty(GroupName))
      action = new SignalRMessageAction(MethodName)
      {
        Arguments = actionArguments,
        GroupName = GroupName
      };

    else if (!string.IsNullOrEmpty(ConnectionId))
      action = new SignalRMessageAction(MethodName)
      {
        Arguments = actionArguments,
        ConnectionId = ConnectionId
      };

    else
      throw new ArgumentNullException("Missing target id type for method");

    return action;
  }
}