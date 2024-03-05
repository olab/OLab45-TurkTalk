using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OLab.TurkTalk.Data.Models;

[Table("ttalk_room_session")]
[Index("RoomId", Name = "fk_rs_r_idx")]
public partial class TtalkRoomSession
{
  [Key]
  [Column("id", TypeName = "int(10) unsigned")]
  public uint Id { get; set; }

  [Column("room_id", TypeName = "int(10) unsigned")]
  public uint RoomId { get; set; }

  [Column("seq_no", TypeName = "int(10)")]
  public int SeqNo { get; set; }

  [Column("attendee_id", TypeName = "int(10) unsigned")]
  public uint? AttendeeId { get; set; }

  [ForeignKey("RoomId")]
  [InverseProperty("TtalkRoomSessions")]
  public virtual TtalkTopicRoom Room { get; set; }

  [InverseProperty("RoomSession")]
  public virtual ICollection<TtalkSessionConversation> TtalkSessionConversations { get; } = new List<TtalkSessionConversation>();
}
