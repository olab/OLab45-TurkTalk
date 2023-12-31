using OLab.TurkTalk.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;
public class TopicParticipant
{
  public string RoomLearnerSessionChannel { get { return $"{TopicId}//{SessionId}//session"; } }
  public string RoomLearnersChannel { get { return $"{TopicId}//{RoomId}//learners"; } }

  public TopicParticipant()
  {

  }
  public TopicParticipant(RegisterParticipantPayload payload)
  {
    SessionId = payload.ContextId;
    TokenIssuer = payload.UserToken.TokenIssuer;
    UserId = payload.UserToken.UserId;
    UserName = payload.UserToken.UserName;
    NickName = payload.UserToken.NickName;
    ConnectionId = payload.ConnectionId;
  }

  public string SessionId { get; set; }
  public string TokenIssuer { get; set; }
  public string UserId { get; set; }
  public string UserName { get; set; }
  public string NickName { get; set; }
  public uint Id { get; set; }
  public uint TopicId { get; set; }
  public string ConnectionId { get; set; }
  public uint RoomId { get; set; }
  public uint SeatNumber { get; set; }

  public override string ToString()
  {
    return $"{UserId}//{UserName}//{TokenIssuer}//{SessionId}";
  }
}
