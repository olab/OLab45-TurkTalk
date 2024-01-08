using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class AtriumLearner
{
  public string ConnectionId { get; set; }
  public string NickName { get; set; }
  public string SessionId { get; set; }
  public string TokenIssuer { get; set; }
  public string UserId { get; set; }
  public string UserName { get; set; }
  public uint Id { get; set; }
  public uint? TopicId { get; set; }

  public AtriumLearner(TtalkTopicParticipant source)
  {
    ConnectionId = source.ConnectionId;
    NickName = source.NickName;
    SessionId = source.SessionId;
    TokenIssuer = source.TokenIssuer;
    UserId = source.UserId;
    UserName = source.UserName;
    Id = source.Id;
    TopicId = source.TopicId;
  }
}

public class AtriumUpdateMethod : TTalkMethod
{
  public IList<AtriumLearner> Attendees { get; set; }

  public AtriumUpdateMethod(
    IOLabConfiguration configuration,
    string groupName,
    IList<TtalkTopicParticipant> learners) : base(configuration, groupName, "atriumupdate")
  {
    Attendees = new List<AtriumLearner>();

    // null out navigation properties that are circular
    foreach (var learner in learners)
      Attendees.Add(new AtriumLearner(learner));
  }

  public override object Arguments()
  {
    return this;
  }

  public override string ToString()
  {
    var sb = new StringBuilder();
    sb.AppendLine(GroupName);
    foreach (var attendee in Attendees)
      sb.AppendLine($" atrium: {attendee.ToString()}");
    return sb.ToString();
  }
}