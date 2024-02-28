using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OLab.TurkTalk.Data.Models;

[Table("ttalk_conference")]
public partial class TtalkConference
{
  [Key]
  [Column("id", TypeName = "int(11) unsigned")]
  public uint Id { get; set; }

  [Required]
  [Column("name")]
  [StringLength(45)]
  public string Name { get; set; }

  [InverseProperty("Conference")]
  public virtual ICollection<TtalkConferenceTopic> TtalkConferenceTopics { get; } = new List<TtalkConferenceTopic>();
}
