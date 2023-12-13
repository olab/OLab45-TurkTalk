﻿using System;
using System.Collections.Generic;

namespace OLab.TurkTalk.Data.Models;

public partial class TtalkRoomSession
{
  public uint Id { get; set; }

  public uint RoomId { get; set; }

  public virtual TtalkTopicRoom Room { get; set; } = null!;
}
