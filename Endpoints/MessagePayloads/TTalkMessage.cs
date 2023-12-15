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

public class NewConnectionCommand : TTalkMessage
{
  private readonly IOLabAuthentication _auth;
  public string UserKey { get; set; }

  public NewConnectionCommand(
    IOLabConfiguration configuration,
    string connectionId,
    IOLabAuthentication auth) : base(
      configuration,
      connectionId,
      "newConnection")
  {
    _auth = auth;

    UserKey = new UserInfoEncoder().EncryptUser(
      Configuration.GetAppSettings().Secret,
      _auth.Claims["id"],
      _auth.Claims[ClaimTypes.Name],
      _auth.Claims["name"],
      _auth.Claims["iss"]);
  }

  public override object Arguments()
  {
    return this;
  }
}