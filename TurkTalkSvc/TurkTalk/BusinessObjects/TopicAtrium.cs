using Common.Utils;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OLabWebAPI.TurkTalk.BusinessObjects
{
    public class TopicAtrium
    {
        public IDictionary<string, Learner> AtriumLearners;
        //public IDictionary<string, AtriumParticipant> AtriumLearners;
        private readonly ILogger _logger;
        private readonly Topic _topic;

        public TopicAtrium(ILogger logger, Topic topic)
        {
            _logger = logger;
            _topic = topic;
            AtriumLearners = new ConcurrentDictionary<string, Learner>();
        }

        private static string GetDictionaryKey(Participant participant)
        {
            return participant.UserId;
        }

        /// <summary>
        /// Get list of Participant
        /// </summary>
        /// <returns>List of Participant group strings</returns>
        public IList<Learner> GetContents()
        {
            return AtriumLearners.Values.ToList();
        }

        /// <summary>
        /// Test if Participant already exists in atrium
        /// </summary>
        /// <param name="name">Participant name</param>
        /// <returns>true, if exists</returns>
        public bool Contains(Participant participant)
        {
            return AtriumLearners.ContainsKey(GetDictionaryKey(participant));
        }

        /// <summary>
        /// Test if Participant name already exists in atrium
        /// </summary>
        /// <param name="name">Participant name (userId)</param>
        /// <returns>true, if exists</returns>
        public bool Contains(string name)
        {
            return AtriumLearners.ContainsKey(name);
        }

        /// <summary>
        /// Get Participant from atrium
        /// </summary>
        /// <param name="name">Participant name</param>
        /// <returns>true, if exists</returns>
        public Learner Get(string name)
        {
            if (AtriumLearners.ContainsKey(name))
                return AtriumLearners[name];

            return null;
        }

        /// <summary>
        /// Remove Participant from atrium
        /// </summary>
        /// <param name="participantName">Participant name</param>
        internal bool Remove(Participant participant)
        {
            // search atrium by user id
            var foundInAtrium = AtriumLearners.ContainsKey(GetDictionaryKey(participant));
            if (foundInAtrium)
            {
                AtriumLearners.Remove(GetDictionaryKey(participant));
                _logger.LogDebug($"Removing Participant '{participant.UserId}' ({participant.ConnectionId}) from '{_topic.Name}' atrium");
            }

            Dump();

            return foundInAtrium;
        }

        /// <summary>
        /// Remove connection id from atrium
        /// </summary>
        /// <param name="connectionId">Connection id to search for</param>
        internal bool Remove(string connectionId)
        {
            foreach (Learner item in AtriumLearners.Values)
            {
                if (item.ConnectionId == connectionId)
                    return Remove(item);
            }

            return false;
        }

        /// <summary>
        /// Add/update Participant to atrium
        /// </summary>
        /// <param name="participant">Participant to add</param>
        /// <returns>true if Participant replaced (versus just added)</returns>
        public bool Upsert(Learner participant)
        {
            var replaced = false;

            // remove if already exists
            if (Contains(GetDictionaryKey(participant)))
            {
                AtriumLearners.Remove(GetDictionaryKey(participant));
                replaced = true;
            }

            Add(participant);
            Dump();

            return replaced;
        }

        /// <summary>
        /// Add Participant to atrium
        /// </summary>
        /// <param name="participant">Participant to add</param>
        internal void Add(Learner participant)
        {
            _logger.LogDebug($"Adding Participant '{participant.NickName}({GetDictionaryKey(participant)})' to '{_topic.Name}' atrium");
            AtriumLearners.Add(GetDictionaryKey(participant), participant);
        }

        private void Dump()
        {
            _logger.LogDebug($"Atrium contents");
            foreach (Learner item in AtriumLearners.Values)
                _logger.LogDebug($"  {item.CommandChannel} ({ConnectionId.Shorten(item.ConnectionId)})");
        }
    }
}
