using Dawn;
using OLab.Access.Interfaces;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Utils;
using System.Security.Claims;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class NewConnectionMethod : TTalkMethod
{
  private readonly IOLabAuthentication _auth;
  public string UserKey { get; set; }

  public NewConnectionMethod(
    IOLabConfiguration configuration,
    string connectionId,
    uint topicId,
    IOLabAuthentication auth) : base(
      configuration,
      connectionId,
      "newConnection")
  {
    Guard.Argument(auth).NotNull(nameof(auth));

    _auth = auth;

    UserKey = new UserToken().EncryptToken(
      Configuration.GetAppSettings().Secret,
      _auth.Claims["id"],
      _auth.Claims[ClaimTypes.Name],
      _auth.Claims["name"],
      _auth.Claims["iss"],
      topicId);
  }

  public override object Arguments()
  {
    return this;
  }

  public override string ToString()
  {
    return UserKey;
  }
}