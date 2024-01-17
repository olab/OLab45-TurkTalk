namespace OLab.TurkTalk.Data.Models;
public partial class TtalkTopicParticipant
{
  public override string ToString()
  {
    return $"{Id}: {UserId}//{UserName}//{TokenIssuer}//{SessionId}";

  }
}
