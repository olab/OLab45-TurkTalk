using OLab.Common.Interfaces;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using System.Text;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class AtriumUpdateMethod : TTalkMethod
{
  public IList<TopicParticipant> Attendees { get; set; }

  public AtriumUpdateMethod(
    IOLabConfiguration configuration,
    string groupName,
    IList<TopicParticipant> learners) : base(configuration, groupName, "atriumupdate")
  {
    Attendees = learners;
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
      sb.AppendLine($" {attendee.ToString()}");
    return sb.ToString();
  }
}