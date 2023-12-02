using System;
using System.Collections.Generic;

namespace OLab.TurkTalk.Models;

public partial class TtalkAtriumUser
{
    public uint Id { get; set; }

    public uint AtriumId { get; set; }

    public string UserId { get; set; } = null!;

    public string UserIdIssuer { get; set; } = null!;

    public virtual TtalkTopicAtrium Atrium { get; set; } = null!;
}
