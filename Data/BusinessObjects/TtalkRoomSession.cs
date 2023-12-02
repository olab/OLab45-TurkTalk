using System;
using System.Collections.Generic;

namespace OLab.TurkTalk.Models;

public partial class TtalkRoomSession
{
    public uint Id { get; set; }

    public uint RoomId { get; set; }

    public virtual TtalkTopicRoom Room { get; set; } = null!;
}
