namespace OLab.TurkTalk.Data.BusinessObjects;
public partial class TtalkTopicParticipant
{
  public override string ToString()
  {
    return $"{Id}: {UserId}//{UserName}//{TokenIssuer}//{SessionId}";

  }
}
