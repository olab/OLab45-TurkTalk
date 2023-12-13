using System;
using System.Collections.Generic;

namespace OLab.TurkTalk.Data.Models;

public partial class TtalkAtriumAttendee
{
  public uint Id { get; set; }

  public uint AtriumId { get; set; }

  public uint AttendeeId { get; set; }

  public virtual TtalkTopicAtrium Atrium { get; set; } = null!;
}
