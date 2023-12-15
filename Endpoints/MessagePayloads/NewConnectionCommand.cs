using OLab.Access.Interfaces;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Endpoints.Utils;
using System.Security.Claims;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

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