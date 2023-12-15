using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OLab.TurkTalk.Data.Models;

[Table("ttalk_atrium_attendee")]
[Index("AtriumId", Name = "fk_au_a_idx")]
[Index("AttendeeId", Name = "fk_au_a_idx1")]
public partial class TtalkAtriumAttendee
{
    [Key]
    [Column("id", TypeName = "int(10) unsigned")]
    public uint Id { get; set; }

    [Column("atrium_id", TypeName = "int(10) unsigned")]
    public uint AtriumId { get; set; }

    [Column("attendee_id", TypeName = "int(10) unsigned")]
    public uint AttendeeId { get; set; }

    [ForeignKey("AtriumId")]
    [InverseProperty("TtalkAtriumAttendees")]
    public virtual TtalkTopicAtrium Atrium { get; set; }

    [ForeignKey("AttendeeId")]
    [InverseProperty("TtalkAtriumAttendees")]
    public virtual TtalkAttendee Attendee { get; set; }
}
