using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OLab.TurkTalk.Data.BusinessObjects;

[Table("ttalk_conference_topic")]
[Index("ConferenceId", Name = "fk_ct_c_idx")]
public partial class TtalkConferenceTopic
{
  [Key]
  [Column("id", TypeName = "int(11) unsigned")]
  public uint Id { get; set; }

  [Required]
  [Column("name")]
  [StringLength(45)]
  public string Name { get; set; }

  [Column("conference_id", TypeName = "int(11) unsigned")]
  public uint ConferenceId { get; set; }

  [Column("map_id", TypeName = "int(11) unsigned")]
  public uint MapId { get; set; }

  [Column("created_at", TypeName = "datetime")]
  public DateTime CreatedAt { get; set; }

  [Column("lastused_at", TypeName = "datetime")]
  public DateTime LastusedAt { get; set; }

  [ForeignKey("ConferenceId")]
  [InverseProperty("TtalkConferenceTopics")]
  public virtual TtalkConference Conference { get; set; }

  [InverseProperty("Topic")]
  public virtual ICollection<TtalkTopicParticipant> TtalkTopicParticipants { get; } = new List<TtalkTopicParticipant>();

  [InverseProperty("Topic")]
  public virtual ICollection<TtalkTopicRoom> TtalkTopicRooms { get; } = new List<TtalkTopicRoom>();
}
