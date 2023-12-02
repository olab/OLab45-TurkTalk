using System;
using System.Collections.Generic;

namespace OLab.TurkTalk.Models;

public partial class TtalkTopicRoom
{
    public uint Id { get; set; }

    public string Name { get; set; } = null!;

    public uint TopicId { get; set; }

    public uint ModeratorId { get; set; }

    public virtual TtalkModerator Moderator { get; set; } = null!;

    public virtual TtalkConferenceTopic Topic { get; set; } = null!;

    public virtual ICollection<TtalkRoomSession> TtalkRoomSessions { get; } = new List<TtalkRoomSession>();
}
