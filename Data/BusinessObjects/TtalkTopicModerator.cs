using System;
using System.Collections.Generic;

namespace OLab.TurkTalk.Models;

public partial class TtalkTopicModerator
{
    public uint Id { get; set; }

    public uint ModeratorId { get; set; }

    public uint TopicId { get; set; }

    public virtual TtalkModerator Moderator { get; set; } = null!;

    public virtual TtalkConferenceTopic Topic { get; set; } = null!;
}
