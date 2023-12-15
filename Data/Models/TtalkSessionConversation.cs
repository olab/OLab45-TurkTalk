using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OLab.TurkTalk.Data.Models;

[Table("ttalk_session_conversation")]
[Index("AttendeeId", Name = "fk_tsc_ta_idx")]
[Index("RoomSessionId", Name = "fk_tsc_trs_idx")]
public partial class TtalkSessionConversation
{
    [Key]
    [Column("id", TypeName = "int(10) unsigned")]
    public uint Id { get; set; }

    [Required]
    [Column("session_id")]
    [StringLength(45)]
    public string SessionId { get; set; }

    [Column("attendee_id", TypeName = "int(10) unsigned")]
    public uint AttendeeId { get; set; }

    [Column("room_session_id", TypeName = "int(10) unsigned")]
    public uint RoomSessionId { get; set; }

    [Column("start_date", TypeName = "datetime")]
    public DateTime StartDate { get; set; }

    [Column("end_date", TypeName = "datetime")]
    public DateTime? EndDate { get; set; }

    [Column("is_attendee_speaker", TypeName = "tinyint(4)")]
    public sbyte IsAttendeeSpeaker { get; set; }

    [ForeignKey("AttendeeId")]
    [InverseProperty("TtalkSessionConversations")]
    public virtual TtalkAttendee Attendee { get; set; }

    [ForeignKey("RoomSessionId")]
    [InverseProperty("TtalkSessionConversations")]
    public virtual TtalkRoomSession RoomSession { get; set; }
}
