using OLab.Api.Model;
using System;

namespace OLab.TurkTalk.Service.Azure.BusinessObjects;

public class TurkTalkQualifiedName
{
  public string ConferenceName { get; set; }
  public string TopicName { get; set; }
  public string RoomName { get; set; }
  public int? RoomInstance { get; set; }
  public int? ChatSlot { get; set; }
  public string UserName { get; set; }

  public const char Separator = ':';

  public bool HasTopic() { return !string.IsNullOrEmpty(TopicName); }
  public bool HasRoom() { return !string.IsNullOrEmpty(RoomName); }
  public bool HasRoomInstance() { return RoomInstance.HasValue; }

  public static TurkTalkQualifiedName Parse(string name)
  {
    var qualifiedName = new TurkTalkQualifiedName();

    var roomParts = name.Split(Separator);

    if (roomParts.Length >= 1)
      qualifiedName.ConferenceName = roomParts[0];

    if (roomParts.Length >= 2)
      qualifiedName.TopicName = roomParts[1];

    if (roomParts.Length >= 3)
      qualifiedName.RoomName = roomParts[2];

    if (roomParts.Length >= 4)
      qualifiedName.RoomInstance = 
        !string.IsNullOrEmpty(roomParts[3]) ? Convert.ToInt32(roomParts[3]) : null;

    if (roomParts.Length >= 5)
      qualifiedName.ChatSlot = 
        !string.IsNullOrEmpty(roomParts[4]) ? Convert.ToInt32(roomParts[4]) : null;

    if (roomParts.Length >= 6)
      qualifiedName.UserName = roomParts[5];

    return qualifiedName;

  }

  public static TurkTalkQualifiedName Parse(TTalkParticipant participant)
  {
    var qualifiedName = Parse(participant.RoomName);
    return qualifiedName;
  }

  public override string ToString()
  {
    return $"{ConferenceName}{Separator}{TopicName}{Separator}{RoomName}{Separator}{RoomInstance}{Separator}{ChatSlot}{Separator}{UserName}";
  }
}
