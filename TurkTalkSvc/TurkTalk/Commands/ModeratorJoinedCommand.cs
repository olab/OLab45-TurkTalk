using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OLabWebAPI.TurkTalk.Commands
{
    public class ModeratorJoinedCommand : CommandMethod
    {
        /// <summary>
        /// Defined a Moderator Joined command method
        /// </summary>
        public string ModeratorName { get; set; }
        public ModeratorJoinedCommand(string groupName, string moderatorName) : base(groupName, "moderatorjoined")
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