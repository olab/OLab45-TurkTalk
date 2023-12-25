using OLab.Common.Interfaces;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using System.Text;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class AtriumUpdateMethod : TTalkMethod
{
  private IOLabConfiguration _configuration;
  public string TopicName { get; set; }
  public IList<RegisterParticipantPayload> Attendees { get; set; }

  public AtriumUpdateMethod(
    IOLabConfiguration configuration,
    string name,
    IList<RegisterParticipantPayload> attendees) : base(configuration, null, "atriumupdate")
  {
    _configuration = configuration;
    TopicName = name;
    Attendees = attendees;
  }

  public override object Arguments()
  {
    return this;
  }

  public override string ToString()
  {
    var sb = new StringBuilder();
    sb.AppendLine(TopicName);
    foreach (var attendee in Attendees)
      sb.AppendLine($" {attendee.ToString()}");
    return sb.ToString();
  }
}