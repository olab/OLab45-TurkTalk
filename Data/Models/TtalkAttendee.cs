﻿using System;
using System.Collections.Generic;

namespace OLab.TurkTalk.Data.BusinessObjects;

public partial class TtalkAttendee
{
  public uint Id { get; set; }

  public string UserId { get; set; } = null!;

  public string UserIdIssuer { get; set; } = null!;
}