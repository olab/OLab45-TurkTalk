using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OLab.TurkTalk.Data.Models;

[Table("ttalk_atrium_attendee")]
[Index("TopicId", Name = "fk_ttalk_atrium_attendee_ttalk_conference_topic1_idx")]
public partial class TtalkAtriumAttendee
{
    [Key]
    [Column("id", TypeName = "int(10) unsigned")]
    public uint Id { get; set; }

    [Required]
    [Column("user_id")]
    [StringLength(45)]
    public string UserId { get; set; }

    [Required]
    [Column("user_name")]
    [StringLength(45)]
    public string UserName { get; set; }

    [Required]
    [Column("token_issuer")]
    [StringLength(45)]
    public string TokenIssuer { get; set; }

    [Column("topic_id", TypeName = "int(11) unsigned")]
    public uint TopicId { get; set; }

    [ForeignKey("TopicId")]
    [InverseProperty("TtalkAtriumAttendees")]
    public virtual TtalkConferenceTopic Topic { get; set; }
}
