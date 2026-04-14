using LinkUp254.Features.Events.models;
using LinkUp254.Features.Groups.Models;
using LinkUp254.Features.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LinkUp254.Database
{
    public class LinkUpContext : DbContext
    {
        public LinkUpContext(DbContextOptions<LinkUpContext> options) : base(options) { }

        // Main Entities
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Event> Events { get; set; } = null!;
        public DbSet<EventAttendee> EventAttendees { get; set; } = null!;
        public DbSet<Ticket> Tickets { get; set; } = null!;

        // Auth / OTP
        public DbSet<OtpCodes> OtpCodes { get; set; } = null!;

        // Shared
        public DbSet<UserInterest> UserInterests { get; set; } = null!;
        public DbSet<Interest> Interests { get; set; } = null!;
        public DbSet<EventInterest> EventInterests { get; set; } = null!;

        // Groups
        public DbSet<Group> Groups { get; set; } = null!;
        public DbSet<GroupMember> GroupMembers { get; set; } = null!;
        public DbSet<GroupEvent> GroupEvents { get; set; } = null!;
        public DbSet<GroupChat> GroupChats { get; set; } = null!;
        public DbSet<GroupMessage> GroupMessages { get; set; } = null!;
        public DbSet<GroupSettings> GroupSettings { get; set; } = null!;
        public DbSet<GroupRule> GroupRules { get; set; } = null!;
        public DbSet<GroupJoinRequest> GroupJoinRequests { get; set; } = null!;

        // Discussions
        public DbSet<GroupDiscussion> GroupDiscussions { get; set; } = null!;
        public DbSet<GroupDiscussionReply> GroupDiscussionReplies { get; set; } = null!;
        public DbSet<GroupDiscussionReaction> GroupDiscussionReactions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //  EVENT 
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasIndex(e => e.OrganizerId);
                entity.HasIndex(e => e.City);
                entity.HasIndex(e => e.Country);
                entity.HasIndex(e => e.StartTime);
                entity.HasIndex(e => new { e.IsActive, e.IsPublished });

                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.Location).HasMaxLength(500);
                entity.Property(e => e.CoverImage).HasMaxLength(500);

                
                entity.HasOne(e => e.Organizer)
                      .WithMany(u => u.EventsHosted)
                      .HasForeignKey(e => e.OrganizerId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            //  EVENT ATTENDEE 
            modelBuilder.Entity<EventAttendee>()
                .HasKey(ea => new { ea.EventId, ea.UserId });

            modelBuilder.Entity<EventAttendee>()
                .HasOne(ea => ea.Event)
                .WithMany(e => e.EventAttendees)
                .HasForeignKey(ea => ea.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventAttendee>()
                .HasOne(ea => ea.User)
                .WithMany(u => u.EventAttendees)
                .HasForeignKey(ea => ea.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //  EVENT INTEREST 
            modelBuilder.Entity<EventInterest>(entity =>
            {
                entity.HasKey(ei => new { ei.EventId, ei.InterestId });

                entity.HasOne(ei => ei.Event)
                      .WithMany(e => e.EventInterests)
                      .HasForeignKey(ei => ei.EventId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ei => ei.Interest)
                      .WithMany(i => i.EventInterests)
                      .HasForeignKey(ei => ei.InterestId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(ei => ei.Weight).HasDefaultValue(1f);
            });

            //  GROUP EVENT 
            modelBuilder.Entity<GroupEvent>(entity =>
            {
                entity.HasIndex(ge => new { ge.GroupId, ge.EventId }).IsUnique();

                entity.HasOne(ge => ge.Group)
                      .WithMany(g => g.GroupEvents)
                      .HasForeignKey(ge => ge.GroupId)
                      .OnDelete(DeleteBehavior.Cascade);

               
                entity.HasOne(ge => ge.Event)
                      .WithMany(e => e.GroupEvents)
                      .HasForeignKey(ge => ge.EventId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // GROUP 
            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasIndex(g => g.OrganizerId);
                entity.HasIndex(g => g.City);
                entity.HasIndex(g => g.Country);
                entity.HasIndex(g => g.IsActive);

                entity.Property(g => g.Name).IsRequired().HasMaxLength(200);
                entity.Property(g => g.Description).HasMaxLength(1000);
                entity.Property(g => g.CoverImage).HasMaxLength(500);
                entity.Property(g => g.City).HasMaxLength(100);
                entity.Property(g => g.Country).HasMaxLength(100);
                entity.Property(g => g.Location).HasMaxLength(500);
                entity.Property(g => g.IsActive).HasDefaultValue(true);
                entity.Property(g => g.MemberCount).HasDefaultValue(0);

                entity.HasOne(g => g.Organizer)
                      .WithMany()
                      .HasForeignKey(g => g.OrganizerId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            //  GROUP MEMBER 
            modelBuilder.Entity<GroupMember>(entity =>
            {
                entity.HasIndex(gm => new { gm.GroupId, gm.UserId }).IsUnique();
                entity.HasIndex(gm => gm.UserId);
                entity.HasIndex(gm => gm.IsActive);

                entity.Property(gm => gm.Role).HasMaxLength(50).HasDefaultValue("member");
                entity.Property(gm => gm.IsActive).HasDefaultValue(true);

                entity.HasOne(gm => gm.Group)
                      .WithMany(g => g.GroupMembers)
                      .HasForeignKey(gm => gm.GroupId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(gm => gm.User)
                      .WithMany()
                      .HasForeignKey(gm => gm.UserId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            //  GROUP CHAT 
            modelBuilder.Entity<GroupChat>(entity =>
            {
                entity.HasIndex(gc => gc.GroupId);
                entity.Property(gc => gc.IsActive).HasDefaultValue(true);

                entity.HasOne(gc => gc.Group)
                      .WithOne()
                      .HasForeignKey<GroupChat>(gc => gc.GroupId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            //  GROUP MESSAGE 
            modelBuilder.Entity<GroupMessage>(entity =>
            {
                entity.HasIndex(gm => gm.GroupChatId);
                entity.HasIndex(gm => gm.SenderId);
                entity.HasIndex(gm => gm.SentAt);

                entity.Property(gm => gm.Content).IsRequired().HasMaxLength(2000);
                entity.Property(gm => gm.IsDeleted).HasDefaultValue(false);
                entity.Property(gm => gm.AttachmentUrl).HasMaxLength(500);

                entity.HasOne(gm => gm.GroupChat)
                      .WithMany(gc => gc.Messages)
                      .HasForeignKey(gm => gm.GroupChatId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(gm => gm.Sender)
                      .WithMany()
                      .HasForeignKey(gm => gm.SenderId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            //  GROUP SETTINGS 
            modelBuilder.Entity<GroupSettings>(entity =>
            {
                entity.HasKey(gs => gs.GroupId);
                entity.Property(gs => gs.IsPrivate).HasDefaultValue(false);
                entity.Property(gs => gs.AllowMemberInvites).HasDefaultValue(true);
                entity.Property(gs => gs.AllowMemberPosts).HasDefaultValue(true);
                entity.Property(gs => gs.ModerateMessages).HasDefaultValue(false);
                entity.Property(gs => gs.AllowLinks).HasDefaultValue(true);
                entity.Property(gs => gs.AllowMedia).HasDefaultValue(true);

                entity.HasOne(gs => gs.Group)
                      .WithOne()
                      .HasForeignKey<GroupSettings>(gs => gs.GroupId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            //  GROUP RULE
            modelBuilder.Entity<GroupRule>(entity =>
            {
                entity.HasIndex(gr => gr.GroupId);
                entity.HasIndex(gr => gr.Order);
                entity.HasIndex(gr => gr.IsActive);

                entity.Property(gr => gr.Title).IsRequired().HasMaxLength(500);
                entity.Property(gr => gr.Description).HasMaxLength(2000);
                entity.Property(gr => gr.IsActive).HasDefaultValue(true);

                entity.HasOne(gr => gr.Group)
                      .WithMany(g => g.GroupRules)
                      .HasForeignKey(gr => gr.GroupId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            //  GROUP JOIN REQUEST 
            modelBuilder.Entity<GroupJoinRequest>(entity =>
            {
                entity.HasIndex(gjr => new { gjr.GroupId, gjr.UserId }).IsUnique();
                entity.HasIndex(gjr => gjr.Status);
                entity.HasIndex(gjr => gjr.RequestedAt);

                entity.Property(gjr => gjr.Status).HasMaxLength(50).HasDefaultValue("pending");
                entity.Property(gjr => gjr.Message).HasMaxLength(500);
                entity.Property(gjr => gjr.ReviewNotes).HasMaxLength(500);

                entity.HasOne(gjr => gjr.Group)
                      .WithMany()
                      .HasForeignKey(gjr => gjr.GroupId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(gjr => gjr.User)
                      .WithMany()
                      .HasForeignKey(gjr => gjr.UserId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            //  USER INTEREST 
            modelBuilder.Entity<UserInterest>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.InterestId });

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Interest)
                      .WithMany()
                      .HasForeignKey(e => e.InterestId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.SelectedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            //  OTP CODES 
            modelBuilder.Entity<OtpCodes>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Identifier).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ExpiresAt).IsRequired();
                entity.Property(e => e.IsUsed).HasDefaultValue(false);
                entity.Property(e => e.OtpType).HasMaxLength(50).HasDefaultValue("General");
                entity.Property(e => e.AttemptCount).HasDefaultValue(0);

                entity.HasOne(o => o.User)
                      .WithMany()
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // TICKET 
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.Property(t => t.Price).HasPrecision(18, 2);
            });

            //  USER 
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
                entity.Property(u => u.FirstName).HasMaxLength(100);
                entity.Property(u => u.LastName).HasMaxLength(100);
                entity.Property(u => u.PhoneNumber).HasMaxLength(20);
                entity.Property(u => u.Password).IsRequired();
            });

            //  INTEREST 
            modelBuilder.Entity<Interest>(entity =>
            {
                entity.Property(i => i.Name).IsRequired().HasMaxLength(100);
                entity.Property(i => i.Category).HasMaxLength(100);
                entity.Property(i => i.Icon).HasMaxLength(50);
            });
        }

        // Auto timestamping for BaseEntity
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;
                if (entry.State == EntityState.Added)
                    entity.CreatedAt = DateTime.UtcNow;

                entity.UpdatedAt = DateTime.UtcNow;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}