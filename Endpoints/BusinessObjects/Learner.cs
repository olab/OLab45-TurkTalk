using Common.Utils;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OLab.Api.Common.Contracts;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using System;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;

public class Learner : Participant
{
  public const string Prefix = "learner";
  public AttendeePayload Session { get; set; }
  public string ReferringNodeName { get; set; }
  public string SessionId { get; set; }

  public Learner()
  {
  }

  public Learner(AttendeePayload session, HubCallerContext context) : base(context)
  {
    Session = session;
    var roomNameParts = session.RoomName.Split("/");

    TopicName = roomNameParts[0];
    RoomName = TopicName;
    CommandChannel = $"{TopicName}/{Prefix}/{UserId}";
    ReferringNodeName = session.ReferringNode;

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

  public override string GetUniqueKey()
  {
    return $"{UserId}:{ConnectionIdUtils.Shorten(SessionId)}";
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