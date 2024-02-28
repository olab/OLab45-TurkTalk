namespace OLab.TurkTalk.Data.Models;

public partial class TtalkTopicRoom
{
  /// <summary>
  /// Group for room moderator commands (e.g. learner connect/disconnects)
  /// </summary>
  public string RoomModeratorChannel { get { return $"{TopicId}//{Id}//moderators"; } }

  /// <summary>
  /// Group for room learner commands (e.g. moderator connect/disconnects)
  /// </summary>
  public string RoomLearnersChannel { get { return $"{TopicId}//{Id}//learners"; } }

}
