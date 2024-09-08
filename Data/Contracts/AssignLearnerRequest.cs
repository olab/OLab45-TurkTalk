namespace OLab.TurkTalk.Data.Contracts;
public class AssignLearnerRequest : RequestBase
{
  public string LearnerSessionId { get; set; }
  public string ModeratorSessionId { get; set; }
  public uint? SeatNumber { get; set; }
  public uint QuestionId { get; set; }
}
