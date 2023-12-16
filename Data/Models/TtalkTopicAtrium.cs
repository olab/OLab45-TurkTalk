using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OLab.TurkTalk.Data.Models;

[Table("ttalk_topic_atrium")]
[Index("TopicId", Name = "fk_ta_t_idx")]
public partial class TtalkTopicAtrium
{
    [Key]
    [Column("id", TypeName = "int(10) unsigned")]
    public uint Id { get; set; }

    [Column("topic_id", TypeName = "int(11) unsigned")]
    public uint TopicId { get; set; }

    [ForeignKey("TopicId")]
    [InverseProperty("TtalkTopicAtria")]
    public virtual TtalkConferenceTopic Topic { get; set; }
}
