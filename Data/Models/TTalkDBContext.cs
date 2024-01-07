using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace OLab.TurkTalk.Data.Models;

public partial class TTalkDBContext : DbContext
{
    public TTalkDBContext(DbContextOptions<TTalkDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TtalkConference> TtalkConferences { get; set; }

    public virtual DbSet<TtalkConferenceTopic> TtalkConferenceTopics { get; set; }

    public virtual DbSet<TtalkRoomSession> TtalkRoomSessions { get; set; }

    public virtual DbSet<TtalkSessionConversation> TtalkSessionConversations { get; set; }

    public virtual DbSet<TtalkTopicParticipant> TtalkTopicParticipants { get; set; }

    public virtual DbSet<TtalkTopicRoom> TtalkTopicRooms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("latin1_swedish_ci")
            .HasCharSet("latin1");

        modelBuilder.Entity<TtalkConference>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
        });

        modelBuilder.Entity<TtalkConferenceTopic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Conference).WithMany(p => p.TtalkConferenceTopics).HasConstraintName("fk_ct_c");
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

        modelBuilder.Entity<TtalkTopicParticipant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Room).WithMany(p => p.TtalkTopicParticipants)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_ttalk_tra_ttr");

            entity.HasOne(d => d.Topic).WithMany(p => p.TtalkTopicParticipants).HasConstraintName("fk_ttalk_ttp_tct");
        });

        modelBuilder.Entity<TtalkTopicRoom>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Moderator).WithMany(p => p.TtalkTopicRooms)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_ttalk_topic_room_ttalk_room_attendee1");

            entity.HasOne(d => d.Topic).WithMany(p => p.TtalkTopicRooms).HasConstraintName("fk_tr_t");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
