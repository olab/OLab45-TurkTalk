using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OLab.TurkTalk.Data.Models;

public partial class TtalkConferenceTopic
{
  public string TopicModeratorsChannel { get { return $"{Id}//moderators"; } }
}
