namespace OLab.TurkTalk.Data.Models;

public partial class TtalkTopicParticipant
{
  public string RoomLearnerSessionChannel
  {
    get
    {
      uint roomId = 0;
      if (RoomId.HasValue)
        roomId = RoomId.Value;
      return $"{TopicId}//{SessionId}//session";
    }
  }
}
