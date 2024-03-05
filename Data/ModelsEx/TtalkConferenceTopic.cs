namespace OLab.TurkTalk.Data.Models;

public partial class TtalkConferenceTopic
{
  public string TopicModeratorsChannel { get { return $"{Id}//moderators"; } }
}
