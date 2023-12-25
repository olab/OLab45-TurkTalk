using Common.Utils;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OLab.Api.Common.Contracts;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using System;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;

public class TopicLearner : TopicParticipant
{
  public string ChatChannel { get { return $"{TopicId}//{SessionId}//session"; } }
  public string RoomChannel { get { return $"{TopicId}//{RoomId}//learners"; } }

  public TopicLearner(RegisterParticipantPayload payload) : base(payload)
  {    
  }
}