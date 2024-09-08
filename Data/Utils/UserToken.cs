using OLab.Api.Utils;

namespace OLab.TurkTalk.Data.Utils;

public class UserToken
{
  public string UserId { get; set; }
  public string UserName { get; set; }
  public string NickName { get; set; }
  public string TokenIssuer { get; set; }
  public uint TopicId { get; set; }

  public string EncryptToken(
  string secret,
  string userId,
  string userName,
  string nickName,
  string tokenIssuer,
  uint topicId)
  {
    var authString = $"{userId}//{userName}//{nickName}//{tokenIssuer}//{topicId}";
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
    TopicId = Convert.ToUInt32(parts[4]);

    return this;
  }

  public override string ToString()
  {
    var authString = $"{UserId}//{UserName}//{TokenIssuer}";
    return authString;
  }
}
