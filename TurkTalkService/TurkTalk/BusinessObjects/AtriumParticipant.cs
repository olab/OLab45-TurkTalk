using OLab.Api.TurkTalk.BusinessObjects;

namespace OLab.TurkTalkSvc.TurkTalk.BusinessObjects;

public class AtriumParticipant
{
  private readonly string _connectionId;

  public string GroupName { get; private set; }
  public string NickName { get; private set; }
  public string TopicName { get; private set; }

  public AtriumParticipant(Learner learner)
  {
    GroupName = learner.CommandChannel;
    TopicName = learner.TopicName;
    NickName = learner.NickName;
    _connectionId = learner.ConnectionId;
  }

  public override string ToString()
  {
    return $"{NickName}({GroupName}) id: {_connectionId}";
  }
}
