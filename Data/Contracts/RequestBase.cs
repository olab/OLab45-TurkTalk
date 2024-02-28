using OLab.TurkTalk.Data.Utils;

namespace OLab.TurkTalk.Data.Contracts;
public class RequestBase
{
  public string UserKey { get; set; }
  public UserToken UserToken { get; set; }
  public string ConnectionId { get; set; }

  public void DecryptAndRefreshUserToken(string secret)
  {
    UserToken = new UserToken().DecryptToken(secret, UserKey);
  }
}
