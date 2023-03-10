using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OLabWebAPI.TurkTalk.Contracts;
using System;

namespace OLabWebAPI.TurkTalk.BusinessObjects
{
  public class Learner : Participant
  {
    public const string Prefix = "learner";
    public RegisterAttendeePayload Session { get; set; }

    public Learner()
    {
    }

    public Learner(RegisterAttendeePayload session, HubCallerContext context) : base(context)
    {
      Session = session;
      var roomNameParts = session.RoomName.Split("/");

      TopicName = roomNameParts[0];
      RoomName = TopicName;
      CommandChannel = $"{TopicName}/{Prefix}/{UserId}";

      // test if topic and room provided
      if (roomNameParts.Length == 2)
        AssignToRoom(Convert.ToInt32(roomNameParts[1]));
    }

    public Learner(Participant participant)
      : base(participant.TopicName, participant.UserId, participant.NickName, participant.ConnectionId)
    {
      CommandChannel = participant.CommandChannel;
    }

    public Learner(string topicName, string userName = null, string nickName = null, string connectionId = null)
      : base(topicName, userName, nickName, connectionId)
    {
    }

    public static string MakeCommandChannel(Participant source)
    {
      return $"{source.TopicName}/{Prefix}/{source.UserId}";
    }

    public override void AssignToRoom(int index)
    {
      RoomNumber = index;
      RoomName = $"{TopicName}/{index}";
    }
    public string ToJson()
    {
      var rawJson = System.Text.Json.JsonSerializer.Serialize(this);
      return JToken.Parse(rawJson).ToString(Formatting.Indented);
    }
  }
}