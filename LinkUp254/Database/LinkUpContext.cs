using LinkUp254.Features.Events.models;
using LinkUp254.Features.Group.Messages;
using LinkUp254.Features.Shared;
using Microsoft.EntityFrameworkCore;

namespace LinkUp254.Database
{
    public class LinkUpContext : DbContext
    {
        public LinkUpContext(DbContextOptions<LinkUpContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Event> Events { get; set; } = null!;
        public DbSet<EventAttendee> EventAttendees { get; set; } = null!;
        public DbSet<Ticket> Tickets { get; set; } = null!;
        public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
        public DbSet<OtpCodes> OtpCodes { get; set; } = null!;

        //  name for Shared.
        public DbSet<LinkUp254.Features.Shared.UserInterest> UserInterests { get; set; } = null!;
        public DbSet<Interest> Interests { get; set; } = null!;
        public DbSet<EventInterest> EventInterests { get; set; } = null!;

        //  Groups feature DbSets
        public DbSet<LinkUp254.Features.Groups.Models.Group> Groups { get; set; } = null!;
        public DbSet<LinkUp254.Features.Groups.Models.GroupMember> GroupMembers { get; set; } = null!;
        public DbSet<LinkUp254.Features.Groups.Models.GroupEvent> GroupEvents { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // EVENT ATTENDEE 
            modelBuilder.Entity<EventAttendee>()
                .HasKey(ea => new { ea.EventId, ea.UserId });
            modelBuilder.Entity<EventAttendee>()
                .HasOne(ea => ea.Event).WithMany(e => e.EventAttendees).HasForeignKey(ea => ea.EventId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<EventAttendee>()
                .HasOne(ea => ea.User).WithMany(u => u.EventAttendees).HasForeignKey(ea => ea.UserId).OnDelete(DeleteBehavior.Cascade);

            //  EVENT INTEREST
            modelBuilder.Entity<EventInterest>(entity =>
            {
                entity.HasKey(ei => new { ei.EventId, ei.InterestId });
                entity.HasOne(ei => ei.Event).WithMany(e => e.EventInterests).HasForeignKey(ei => ei.EventId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ei => ei.Interest).WithMany(i => i.EventInterests).HasForeignKey(ei => ei.InterestId).OnDelete(DeleteBehavior.Cascade);
                entity.Property(ei => ei.Weight).HasDefaultValue(1f);
            });

            //  EVENT CONFIGURATION 
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasIndex(e => e.OrganizerId);
                entity.HasIndex(e => e.City);
                entity.HasIndex(e => e.Country);
                entity.HasIndex(e => e.StartTime);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsPublished);
                entity.HasIndex(e => new { e.IsActive, e.IsPublished, e.StartTime, e.RelevanceScore });

                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsPublished).HasDefaultValue(false);
                entity.Property(e => e.AttendeeCount).HasDefaultValue(0);
                entity.Property(e => e.ViewCount).HasDefaultValue(0);
                entity.Property(e => e.LikeCount).HasDefaultValue(0);
                entity.Property(e => e.RelevanceScore).HasDefaultValue(0f);

                entity.HasOne(e => e.Organizer)
                    .WithMany()
                    .HasForeignKey(e => e.OrganizerId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.Location).HasMaxLength(500);
                entity.Property(e => e.CoverImage).HasMaxLength(500);
            });

            //  TICKET 
            modelBuilder.Entity<Ticket>().Property(t => t.Price).HasPrecision(18, 2);

            // USER 
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
                entity.Property(u => u.FirstName).HasMaxLength(100);
                entity.Property(u => u.LastName).HasMaxLength(100);
                entity.Property(u => u.PhoneNumber).HasMaxLength(20);
                entity.Property(u => u.Password).IsRequired();
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

                entity.HasIndex(e => new { e.Code, e.Identifier, e.IsUsed, e.ExpiresAt }).HasDatabaseName("IX_OtpCodes_Verification");
                entity.HasIndex(e => e.Identifier).HasDatabaseName("IX_OtpCodes_Identifier");

                entity.HasOne(o => o.User).WithMany().HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.SetNull);
            });

            //  INTEREST 
            modelBuilder.Entity<Interest>(entity =>
            {
                entity.Property(i => i.Name).IsRequired().HasMaxLength(100);
                entity.Property(i => i.Category).HasMaxLength(100);
                entity.Property(i => i.Icon).HasMaxLength(50);
            });

            // USER INTEREST 
            modelBuilder.Entity<LinkUp254.Features.Shared.UserInterest>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.InterestId });
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Interest).WithMany().HasForeignKey(e => e.InterestId).OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.SelectedAt).HasDefaultValueSql("GETUTCDATE()");
            });


            // Group entity configuration
            modelBuilder.Entity<LinkUp254.Features.Groups.Models.Group>(entity =>
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
                entity.Property(g => g.IsActive).HasDefaultValue(true);
                entity.Property(g => g.MemberCount).HasDefaultValue(0);

                entity.HasOne(g => g.Organizer)
                    .WithMany()
                    .HasForeignKey(g => g.OrganizerId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // GroupMember entity configuration
            modelBuilder.Entity<LinkUp254.Features.Groups.Models.GroupMember>(entity =>
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
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // GroupEvent entity configuration
            modelBuilder.Entity<LinkUp254.Features.Groups.Models.GroupEvent>(entity =>
            {
                entity.HasIndex(ge => new { ge.GroupId, ge.EventId }).IsUnique();

                entity.HasOne(ge => ge.Group)
                    .WithMany(g => g.GroupEvents)
                    .HasForeignKey(ge => ge.GroupId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ge => ge.Event)
                    .WithMany()
                    .HasForeignKey(ge => ge.EventId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        //  AUTO TIMESTAMPING 
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;
                if (entry.State == EntityState.Added) entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}