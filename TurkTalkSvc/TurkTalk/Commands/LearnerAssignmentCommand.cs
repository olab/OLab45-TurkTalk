using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OLabWebAPI.TurkTalk.BusinessObjects;

namespace OLabWebAPI.TurkTalk.Commands
{
  public class LearnerAssignmentPayload
  {
    public Participant Learner { get; set; }
    public int SlotIndex { get; set; }
  }

  /// <summary>
  /// Defines a Learner Assignment command method
  /// </summary>
  public class LearnerAssignmentCommand : CommandMethod
  {
    public LearnerAssignmentPayload Data { get; set; }

    public LearnerAssignmentCommand(
      Participant moderator,
      Learner learner) : base(moderator.CommandChannel, "learnerassignment")
    {
      Data = new LearnerAssignmentPayload { Learner = learner, SlotIndex = learner.SlotIndex };
    }

    public override string ToJson()
    {
      var rawJson = System.Text.Json.JsonSerializer.Serialize(this);
      return JToken.Parse(rawJson).ToString(Formatting.Indented);
    }

  }
}