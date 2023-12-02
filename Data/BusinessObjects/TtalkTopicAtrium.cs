using System;
using System.Collections.Generic;

namespace OLab.TurkTalk.Models;

public partial class TtalkTopicAtrium
{
    public uint Id { get; set; }

    public uint TopicId { get; set; }

    public virtual TtalkConferenceTopic Topic { get; set; } = null!;

    public virtual ICollection<TtalkAtriumAttendee> TtalkAtriumAttendees { get; } = new List<TtalkAtriumAttendee>();
}
