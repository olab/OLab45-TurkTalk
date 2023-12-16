using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace OLab.TurkTalk.Data.Models;

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

    public virtual DbSet<TtalkConference> TtalkConferences { get; set; }

    public virtual DbSet<TtalkConferenceTopic> TtalkConferenceTopics { get; set; }

    public virtual DbSet<TtalkModerator> TtalkModerators { get; set; }

    public virtual DbSet<TtalkRoomSession> TtalkRoomSessions { get; set; }

    public virtual DbSet<TtalkSessionConversation> TtalkSessionConversations { get; set; }

    public virtual DbSet<TtalkTopicAtrium> TtalkTopicAtria { get; set; }

    public virtual DbSet<TtalkTopicRoom> TtalkTopicRooms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("latin1_swedish_ci")
            .HasCharSet("latin1");

        modelBuilder.Entity<TtalkAtriumAttendee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Topic).WithMany(p => p.TtalkAtriumAttendees)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ttalk_atrium_attendee_ttalk_conference_topic1");
        });

        modelBuilder.Entity<TtalkConference>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<TtalkConferenceTopic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Conference).WithMany(p => p.TtalkConferenceTopics).HasConstraintName("fk_ct_c");
        });

        modelBuilder.Entity<TtalkModerator>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<TtalkRoomSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Room).WithMany(p => p.TtalkRoomSessions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_rs_r");
        });

        modelBuilder.Entity<TtalkSessionConversation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.RoomSession).WithMany(p => p.TtalkSessionConversations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tsc_trs");
        });

        modelBuilder.Entity<TtalkTopicAtrium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Topic).WithMany(p => p.TtalkTopicAtria).HasConstraintName("fk_ta_t");
        });

        modelBuilder.Entity<TtalkTopicRoom>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Moderator).WithMany(p => p.TtalkTopicRooms)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tr_m");

            entity.HasOne(d => d.Topic).WithMany(p => p.TtalkTopicRooms).HasConstraintName("fk_tr_t");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
