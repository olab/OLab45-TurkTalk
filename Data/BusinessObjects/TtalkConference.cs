using System;
using System.Collections.Generic;

namespace OLab.TurkTalk.Models;

public partial class TtalkConference
{
    public uint Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<TtalkConferenceTopic> TtalkConferenceTopics { get; } = new List<TtalkConferenceTopic>();
}
