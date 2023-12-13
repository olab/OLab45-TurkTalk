using System;
using System.Collections.Generic;

namespace OLab.TurkTalk.Data.Models;

public partial class TtalkModerator
{
  public uint Id { get; set; }

  public string UserId { get; set; } = null!;

  public string UserIdIssuer { get; set; } = null!;

  public virtual ICollection<TtalkTopicModerator> TtalkTopicModerators { get; } = new List<TtalkTopicModerator>();

  public virtual ICollection<TtalkTopicRoom> TtalkTopicRooms { get; } = new List<TtalkTopicRoom>();
}
