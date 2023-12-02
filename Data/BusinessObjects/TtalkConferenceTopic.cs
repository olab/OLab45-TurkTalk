using System;
using System.Collections.Generic;

namespace OLab.TurkTalk.Models;

public partial class TtalkConferenceTopic
{
    public uint Id { get; set; }

    public string Name { get; set; } = null!;

    public uint ConferenceId { get; set; }

    public virtual TtalkConference Conference { get; set; } = null!;

    public virtual ICollection<TtalkTopicAtrium> TtalkTopicAtria { get; } = new List<TtalkTopicAtrium>();

    public virtual ICollection<TtalkTopicModerator> TtalkTopicModerators { get; } = new List<TtalkTopicModerator>();

    public virtual ICollection<TtalkTopicRoom> TtalkTopicRooms { get; } = new List<TtalkTopicRoom>();
}
