using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OLab.TurkTalk.Endpoints.Utils;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class RegisterParticipantPayload
{
  public string ContextId { get; set; }
  public uint MapId { get; set; }
  public uint NodeId { get; set; }
  public uint QuestionId { get; set; }

  public string ConnectionId { get; set; }
  public string UserKey { get; set; }

  public UserToken UserToken { get; set; }

  public void RefreshUserToken(string secret)
  {
    UserToken = new UserToken().DecryptToken( secret, UserKey );
  }

  public string ToJson()
  {
    var rawJson = System.Text.Json.JsonSerializer.Serialize(this);
    return JToken.Parse(rawJson).ToString(Formatting.Indented);
  }

  public override string ToString()
  {
    return $"{UserKey}//{ContextId}";
  }

}
