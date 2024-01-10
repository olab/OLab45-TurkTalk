using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class AtriumUpdateMethod : TTalkMethod
{
  public IList<TopicParticipantDto> Attendees { get; set; }

  public AtriumUpdateMethod(
    IOLabConfiguration configuration,
    string groupName,
    IList<TtalkTopicParticipant> learners) : base(configuration, groupName, "atriumupdate")
  {
    Attendees = new List<TopicParticipantDto>();

    // null out navigation properties that are circular
    foreach (var learner in learners)
      Attendees.Add(new TopicParticipantDto(learner));
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