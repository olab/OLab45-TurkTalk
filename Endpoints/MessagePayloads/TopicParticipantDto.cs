using OLab.TurkTalk.Data.Models;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class TopicParticipantDto
{
  public string ConnectionId { get; set; }
  public string NickName { get; set; }
  public string SessionId { get; set; }
  public string TokenIssuer { get; set; }
  public string UserId { get; set; }
  public string UserName { get; set; }
  public uint Id { get; set; }
  public uint? TopicId { get; set; }
  public string GroupName { get; set; }

  public TopicParticipantDto(TtalkTopicParticipant source)
  {
    ConnectionId = source.ConnectionId;
    NickName = source.NickName;
    SessionId = source.SessionId;
    TokenIssuer = source.TokenIssuer;
    UserId = source.UserId;
    UserName = source.UserName;
    Id = source.Id;
    TopicId = source.TopicId;
    GroupName = source.RoomLearnerSessionChannel;
  }
}
