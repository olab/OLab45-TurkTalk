using Microsoft.AspNetCore.SignalR;
using System;

namespace OLabWebAPI.TurkTalk.BusinessObjects
{
    public class Moderator : Participant
  {
    private const string _prefix = "moderator";

    public Moderator()
    {

    }

    public Moderator(string roomName, HubCallerContext context) : base(context)
    {
      var roomNameParts = roomName.Split("/");

      TopicName = roomNameParts[0];
      RoomName = TopicName;
      CommandChannel = $"{TopicName}/{_prefix}/{UserId}";

      // test for topic and room
      if (roomNameParts.Length == 2)
        AssignToRoom(Convert.ToInt32(roomNameParts[1]));
    }

    public Moderator(string topicName, string userName = null, string nickName = null, string connectionId = null)
    : base(topicName, userName, nickName, connectionId)
    {
    }

    public override void AssignToRoom(int index)
    {
      RoomNumber = index;
      if (RoomNumber.HasValue)
        RoomName = $"{TopicName}/{RoomNumber.Value}";
      else
        RoomName = null;
    }

  }
}
