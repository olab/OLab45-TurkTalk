using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OLabWebAPI.TurkTalk.Commands
{
    public class ModeratorDisconnectedCommand : CommandMethod
    {
        /// <summary>
        /// Defined a Moderator Joined command method
        /// </summary>
        public string ModeratorName { get; set; }
        public ModeratorDisconnectedCommand(string groupName) : base(groupName, "moderatordisconnected")
        {
            ModeratorName = ModeratorName;
        }

        public override string ToJson()
        {
            var rawJson = System.Text.Json.JsonSerializer.Serialize(this);
            return JToken.Parse(rawJson).ToString(Formatting.Indented);
        }

    }
}