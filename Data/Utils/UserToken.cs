using OLab.Api.Utils;
using System;

namespace OLab.TurkTalk.Data.Utils;

public class UserToken
{
  public string UserId { get; set; }
  public string UserName { get; set; }
  public string NickName { get; set; }
  public string TokenIssuer { get; set; }

  public string EncryptToken(
  string secret,
  string userId,
  string userName,
  string nickName,
  string tokenIssuer)
  {
    var authString = $"{userId}//{userName}//{nickName}//{tokenIssuer}";
    return StringUtils.EncryptString(secret, authString);
  }

  public UserToken DecryptToken(string secret, string key)
  {
    var clearText = StringUtils.DecryptString(secret, key);
    var parts = clearText.Split("//");

    UserId = parts[0];
    UserName = parts[1];
    NickName = parts[2];
    TokenIssuer = parts[3];

    return this;
  }

  public override string ToString()
  {
    var authString = $"{UserId}//{UserName}//{TokenIssuer}";
    return authString;
  }
}
