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

  public bool IsInRoom()
  {
    return TopicId.HasValue && RoomId.HasValue;
  }

  public bool IsInTopicAtrium()
  {
    return TopicId.HasValue && !RoomId.HasValue;
  }

  public bool IsRoomLearner()
  {
    return IsInRoom() && SeatNumber.HasValue && ( SeatNumber.Value > 0 );
  }

  public bool IsModerator()
  {
    return IsInRoom() && SeatNumber.HasValue && ( SeatNumber.Value == 0 );
  }
}
