using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OLab.TurkTalk.Data.Models;

[Table("ttalk_moderator")]
public partial class TtalkModerator
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

    [InverseProperty("Moderator")]
    public virtual ICollection<TtalkTopicRoom> TtalkTopicRooms { get; } = new List<TtalkTopicRoom>();
}
