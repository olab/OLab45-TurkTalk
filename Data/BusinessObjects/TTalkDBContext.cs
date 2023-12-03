using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace OLab.TurkTalk.Models;

public partial class TTalkDBContext : DbContext
{
    public TTalkDBContext()
    {
    }

    public TTalkDBContext(DbContextOptions<TTalkDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TtalkAtriumAttendee> TtalkAtriumAttendees { get; set; }

    public virtual DbSet<TtalkAttendee> TtalkAttendees { get; set; }

    public virtual DbSet<TtalkConference> TtalkConferences { get; set; }

    public virtual DbSet<TtalkConferenceTopic> TtalkConferenceTopics { get; set; }

    public virtual DbSet<TtalkModerator> TtalkModerators { get; set; }

    public virtual DbSet<TtalkRoomSession> TtalkRoomSessions { get; set; }

    public virtual DbSet<TtalkTopicAtrium> TtalkTopicAtria { get; set; }

    public virtual DbSet<TtalkTopicModerator> TtalkTopicModerators { get; set; }

    public virtual DbSet<TtalkTopicRoom> TtalkTopicRooms { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=olab45db.mysql.database.azure.com;uid=olab4admin;pwd=<password>;database=olab45;convertzerodatetime=True", Microsoft.EntityFrameworkCore.ServerVersion.Parse("5.7.43-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("latin1_swedish_ci")
            .HasCharSet("latin1");

        modelBuilder.Entity<TtalkAtriumAttendee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ttalk_atrium_attendee");

            entity.HasIndex(e => e.AtriumId, "fk_au_a_idx");

            entity.HasIndex(e => e.AttendeeId, "fk_au_a_idx1");

            entity.Property(e => e.Id)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.AtriumId)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("atrium_id");
            entity.Property(e => e.AttendeeId)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("attendee_id");

            entity.HasOne(d => d.Atrium).WithMany(p => p.TtalkAtriumAttendees)
                .HasForeignKey(d => d.AtriumId)
                .HasConstraintName("fk_au_a");
        });

        modelBuilder.Entity<TtalkAttendee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ttalk_attendee");

            entity.Property(e => e.Id)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.UserId)
                .HasMaxLength(45)
                .HasColumnName("user_id");
            entity.Property(e => e.UserIdIssuer)
                .HasMaxLength(45)
                .HasColumnName("user_id_issuer");
        });

        modelBuilder.Entity<TtalkConference>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ttalk_conference");

            entity.Property(e => e.Id)
                .HasColumnType("int(11) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TtalkConferenceTopic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ttalk_conference_topic");

            entity.HasIndex(e => e.ConferenceId, "fk_ct_c_idx");

            entity.Property(e => e.Id)
                .HasColumnType("int(11) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.ConferenceId)
                .HasColumnType("int(11) unsigned")
                .HasColumnName("conference_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");

            entity.HasOne(d => d.Conference).WithMany(p => p.TtalkConferenceTopics)
                .HasForeignKey(d => d.ConferenceId)
                .HasConstraintName("fk_ct_c");
        });

        modelBuilder.Entity<TtalkModerator>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ttalk_moderator");

            entity.Property(e => e.Id)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.UserId)
                .HasMaxLength(45)
                .HasColumnName("user_id");
            entity.Property(e => e.UserIdIssuer)
                .HasMaxLength(45)
                .HasColumnName("user_id_issuer");
        });

        modelBuilder.Entity<TtalkRoomSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ttalk_room_session");

            entity.HasIndex(e => e.RoomId, "fk_rs_r_idx");

            entity.Property(e => e.Id)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.RoomId)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("room_id");

            entity.HasOne(d => d.Room).WithMany(p => p.TtalkRoomSessions)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_rs_r");
        });

        modelBuilder.Entity<TtalkTopicAtrium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ttalk_topic_atrium");

            entity.HasIndex(e => e.TopicId, "fk_ta_t_idx");

            entity.Property(e => e.Id)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.TopicId)
                .HasColumnType("int(11) unsigned")
                .HasColumnName("topic_id");

            entity.HasOne(d => d.Topic).WithMany(p => p.TtalkTopicAtria)
                .HasForeignKey(d => d.TopicId)
                .HasConstraintName("fk_ta_t");
        });

        modelBuilder.Entity<TtalkTopicModerator>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ttalk_topic_moderator");

            entity.HasIndex(e => e.ModeratorId, "fk_tm_m_idx");

            entity.HasIndex(e => e.TopicId, "fk_tm_t_idx");

            entity.Property(e => e.Id)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.ModeratorId)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("moderator_id");
            entity.Property(e => e.TopicId)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("topic_id");

            entity.HasOne(d => d.Moderator).WithMany(p => p.TtalkTopicModerators)
                .HasForeignKey(d => d.ModeratorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tm_m");

            entity.HasOne(d => d.Topic).WithMany(p => p.TtalkTopicModerators)
                .HasForeignKey(d => d.TopicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tm_t");
        });

        modelBuilder.Entity<TtalkTopicRoom>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ttalk_topic_room");

            entity.HasIndex(e => e.ModeratorId, "fk_tr_m_idx");

            entity.HasIndex(e => e.TopicId, "fk_tr_t_idx");

            entity.Property(e => e.Id)
                .HasColumnType("int(11) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.ModeratorId)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("moderator_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.TopicId)
                .HasColumnType("int(11) unsigned")
                .HasColumnName("topic_id");

            entity.HasOne(d => d.Moderator).WithMany(p => p.TtalkTopicRooms)
                .HasForeignKey(d => d.ModeratorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tr_m");

            entity.HasOne(d => d.Topic).WithMany(p => p.TtalkTopicRooms)
                .HasForeignKey(d => d.TopicId)
                .HasConstraintName("fk_tr_t");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
