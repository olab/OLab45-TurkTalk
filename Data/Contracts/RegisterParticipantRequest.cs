using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OLab.TurkTalk.Data.Contracts;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class RegisterParticipantRequest : RequestBase
{
  public string ContextId { get; set; }
  public uint MapId { get; set; }
  public uint NodeId { get; set; }
  public uint QuestionId { get; set; }

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
