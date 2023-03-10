using OLabWebAPI.TurkTalk.BusinessObjects;
using System.Collections.Generic;

namespace OLabWebAPI.TurkTalk.Contracts
{
    public class ModeratorAssignmentPayload
    {
        public IList<MapNodeListItem> MapNodes { get; set; }
        public Moderator Remote { get; set; }

    }
}
