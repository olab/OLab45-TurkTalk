using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OLabWebAPI.TurkTalk.BusinessObjects;
using System.Collections.Generic;

namespace OLabWebAPI.TurkTalk.Commands
{
    /// <summary>
    /// Defines a Atrium Update command method
    /// </summary>
    public class AtriumUpdateCommand : CommandMethod
    {
        public IList<Learner> Data { get; set; }

        // constructor for targetted group
        public AtriumUpdateCommand(string groupName, IList<Learner> atriumLearners) : base(groupName, "atriumupdate")
        {
            Data = atriumLearners;
        }

        // constructor for all moderators in a topic
        public AtriumUpdateCommand(Topic topic, IList<Learner> atriumLearners) : base(topic.TopicModeratorsChannel, "atriumupdate")
        {
            Data = atriumLearners;
        }

        public override string ToJson()
        {
            var rawJson = System.Text.Json.JsonSerializer.Serialize(this);
            return JToken.Parse(rawJson).ToString(Formatting.Indented);
        }

    }
}