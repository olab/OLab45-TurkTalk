using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OLabWebAPI.TurkTalk.Contracts;

namespace OLabWebAPI.TurkTalk.Commands
{
    public class SystemMessageCommand : Method
    {
        public string Data { get; set; }
        /// <summary>
        /// Defines a Moderator removed command method
        /// </summary>
        public SystemMessageCommand(MessagePayload payload) : base(payload.Envelope.To, "systemmessage")
        {
            Data = payload.Data;
        }

        public override string ToJson()
        {
            var rawJson = System.Text.Json.JsonSerializer.Serialize(this);
            return JToken.Parse(rawJson).ToString(Formatting.Indented);
        }

    }
}