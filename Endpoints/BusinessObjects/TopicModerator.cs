using Common.Utils;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OLab.Api.Common.Contracts;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using System;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;

public class TopicModerator : TopicParticipant
{
  public TopicModerator(RegisterParticipantPayload payload) : base(payload)
  {

  }

  public TopicModerator(TopicParticipant source)
  {
    Id = source.Id;
    TopicId = source.TopicId;
    RoomId = source.RoomId;
    SessionId = source.SessionId;
    TokenIssuer = source.TokenIssuer;
    UserId = source.UserId;
    UserName = source.UserName;
    NickName = source.NickName;
    ConnectionId = source.ConnectionId;
    SeatNumber = source.SeatNumber;
  }
}