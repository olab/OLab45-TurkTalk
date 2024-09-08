using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OLab.TurkTalk.Data.BusinessObjects;

[Table("ttalk_topic_room")]
[Index("TopicId", Name = "fk_tr_t_idx")]
[Index("ModeratorId", Name = "fk_ttalk_topic_room_ttalk_room_attendee1_idx")]
public partial class TtalkTopicRoom
{
  [Key]
  [Column("id", TypeName = "int(11) unsigned")]
  public uint Id { get; set; }

  [Column("topic_id", TypeName = "int(11) unsigned")]
  public uint TopicId { get; set; }

  [Column("moderator_id", TypeName = "int(10) unsigned")]
  public uint? ModeratorId { get; set; }

  [ForeignKey("ModeratorId")]
  [InverseProperty("TtalkTopicRooms")]
  public virtual TtalkTopicParticipant Moderator { get; set; }

  [ForeignKey("TopicId")]
  [InverseProperty("TtalkTopicRooms")]
  public virtual TtalkConferenceTopic Topic { get; set; }

  [InverseProperty("Room")]
  public virtual ICollection<TtalkRoomSession> TtalkRoomSessions { get; } = new List<TtalkRoomSession>();

  [InverseProperty("Room")]
  public virtual ICollection<TtalkTopicParticipant> TtalkTopicParticipants { get; } = new List<TtalkTopicParticipant>();
}
