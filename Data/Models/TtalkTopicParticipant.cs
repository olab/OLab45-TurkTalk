﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OLab.TurkTalk.Data.Models;

[Table("ttalk_topic_participant")]
[Index("RoomId", Name = "fk_ttalk_tra_ttr_idx")]
public partial class TtalkTopicParticipant
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

    [Column("nick_name")]
    [StringLength(45)]
    public string NickName { get; set; }

    [Required]
    [Column("token_issuer")]
    [StringLength(45)]
    public string TokenIssuer { get; set; }

    [Column("room_id", TypeName = "int(11) unsigned")]
    public uint? RoomId { get; set; }

    [Column("seat_number", TypeName = "int(11) unsigned")]
    public uint? SeatNumber { get; set; }

    [Required]
    [Column("session_id")]
    [StringLength(45)]
    public string SessionId { get; set; }

    [Column("connection_id")]
    [StringLength(45)]
    public string ConnectionId { get; set; }

    [ForeignKey("RoomId")]
    [InverseProperty("TtalkTopicParticipants")]
    public virtual TtalkTopicRoom Room { get; set; }

    [InverseProperty("Moderator")]
    public virtual ICollection<TtalkTopicRoom> TtalkTopicRooms { get; } = new List<TtalkTopicRoom>();
}
