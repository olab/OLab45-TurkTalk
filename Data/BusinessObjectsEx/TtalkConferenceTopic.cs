namespace OLab.TurkTalk.Data.BusinessObjects;

public partial class TtalkConferenceTopic
{
  public string TopicModeratorsChannel { get { return $"{Id}//moderators"; } }
}
