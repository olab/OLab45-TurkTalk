using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;
public class TopicParticipant
{
  /// <summary>
  /// Channel for learner session specfic messages and commands (e.g. node jump)
  /// </summary>
  public string RoomLearnerSessionChannel { get { return $"{TopicId}//{RoomId}//{SessionId}//session"; } }

  public TopicParticipant()
  {

  }
  public TopicParticipant(RegisterParticipantRequest payload)
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
