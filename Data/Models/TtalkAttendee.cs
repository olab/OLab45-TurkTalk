using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OLab.TurkTalk.Data.Models;

[Table("ttalk_attendee")]
public partial class TtalkAttendee
{
    [Key]
    [Column("id", TypeName = "int(10) unsigned")]
    public uint Id { get; set; }

    [Required]
    [Column("user_id")]
    [StringLength(45)]
    public string UserId { get; set; }

    [Required]
    [Column("user_id_issuer")]
    [StringLength(45)]
    public string UserIdIssuer { get; set; }

    [InverseProperty("Attendee")]
    public virtual ICollection<TtalkAtriumAttendee> TtalkAtriumAttendees { get; } = new List<TtalkAtriumAttendee>();

    [InverseProperty("Attendee")]
    public virtual ICollection<TtalkRoomSession> TtalkRoomSessions { get; } = new List<TtalkRoomSession>();

    [InverseProperty("Attendee")]
    public virtual ICollection<TtalkSessionConversation> TtalkSessionConversations { get; } = new List<TtalkSessionConversation>();
}
