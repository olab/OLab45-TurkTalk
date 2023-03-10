using Dawn;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OLabWebAPI.TurkTalk.BusinessObjects;

namespace OLabWebAPI.TurkTalk.Commands
{
  public class RoomUnassignmentPayload
  {
    public Participant Participant { get; set; }
    public int SlotIndex { get; set; }
  }
  /// <summary>
  /// Defines a command to remove a connection from a room
  /// </summary>
  public class RoomUnassignmentCommand : CommandMethod
  {
    public RoomUnassignmentPayload Data { get; set; }

    public RoomUnassignmentCommand(string recipientGroupName, Participant participant) : base(recipientGroupName, "learnerunassignment")
    {
      Guard.Argument(participant).NotNull(nameof(participant));
      Data = new RoomUnassignmentPayload 
      { 
        Participant = participant, 
        SlotIndex = participant.SlotIndex 
      };
    }

    public override string ToJson()
    {
      var rawJson = System.Text.Json.JsonSerializer.Serialize(this);
      return JToken.Parse(rawJson).ToString(Formatting.Indented);
    }

  }
}