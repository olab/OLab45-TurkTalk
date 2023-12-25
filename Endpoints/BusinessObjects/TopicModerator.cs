using Common.Utils;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OLab.Api.Common.Contracts;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using System;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;

public class TopicModerator: TopicParticipant
{
  public string ModeratorChannel { get { return $"{TopicId}//{RoomId}//moderator"; } }
  public string ModeratorsChannel { get { return $"{TopicId}//moderators"; } }

  public TopicModerator(RegisterParticipantPayload payload) : base(payload)
  {

  }
}