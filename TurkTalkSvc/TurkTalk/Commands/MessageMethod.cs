using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OLabWebAPI.TurkTalk.Contracts;

namespace OLabWebAPI.TurkTalk.Commands
{
    /// <summary>
    /// Defines a Message command method
    /// </summary>
    public class MessageMethod : Method
    {
        public string Data { get; set; }
        public string SessionId { get; set; }
        public string From { get; set; }

        // message for specific group
        public MessageMethod(MessagePayload payload) : base(payload.Envelope.To, "message")
        {
            Data = payload.Data;
            SessionId = payload.Session.ContextId;
            From = payload.Envelope.From.UserId;
        }
        public override string ToJson()
        {
            var rawJson = System.Text.Json.JsonSerializer.Serialize(this);
            return JToken.Parse(rawJson).ToString(Formatting.Indented);
        }

    }
}