using Dawn;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OLabWebAPI.TurkTalk.Commands
{
    /// <summary>
    /// Defines a command method
    /// </summary>
    public abstract class CommandMethod : Method
    {
        public string Command { get; set; }

        public CommandMethod(string recipientGroupName, string command) : base(recipientGroupName, "Command")
        {
            Guard.Argument(command).NotEmpty(command);
            Command = command;
        }

        public override string ToJson()
        {
            var rawJson = System.Text.Json.JsonSerializer.Serialize(this);
            return JToken.Parse(rawJson).ToString(Formatting.Indented);
        }
    }
}