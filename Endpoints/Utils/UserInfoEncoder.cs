using OLab.Api.TurkTalk.BusinessObjects;
using OLab.Api.Utils;
using System;

namespace OLab.TurkTalk.Endpoints.Utils;

public class UserInfoEncoder
{
  public uint UserId { get; set; }
  public string UserName { get; set; }
  public string NickName { get; set; }
  public string TokenIssuer { get; set; }

  public string EncryptUser(
    string secret,
    string userId,
    string userName,
    string nickName,
    string tokenIssuer)
  {
    var authString = $"{userName}//{nickName}//{tokenIssuer}";
    return StringUtils.EncryptString(secret, authString);
  }

  public Learner DecryptUser(string secret, string key)
  {
    var clearText = StringUtils.DecryptString(secret, key);
    var parts = clearText.Split("//");

    UserName = parts[0];
    NickName = parts[1];
    TokenIssuer = parts[2];

    return new Learner { UserId = UserName, NickName = NickName };
  }
}
