using LinkUp254.Features.Events;
using LinkUp254.Features.Messages;
using LinkUp254.Features.Shared;
using Microsoft.EntityFrameworkCore;

namespace LinkUp254.Database
{
    public class LinkUpContext : DbContext
    {
        public LinkUpContext(DbContextOptions<LinkUpContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Event> Events { get; set; } = null!;
        public DbSet<EventAttendee> EventAttendees { get; set; } = null!;
        public DbSet<Ticket> Tickets { get; set; } = null!;
        public DbSet<ChatMessage> ChatMessages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite key for EventAttendee
            modelBuilder.Entity<EventAttendee>()
                .HasKey(ea => new { ea.EventId, ea.UserId });

            // Relationships
            modelBuilder.Entity<EventAttendee>()
                .HasOne(ea => ea.Event)
                .WithMany(e => e.EventAttendees)
                .HasForeignKey(ea => ea.EventId);

            modelBuilder.Entity<EventAttendee>()
                .HasOne(ea => ea.User)
                .WithMany(u => u.EventAttendees)
                .HasForeignKey(ea => ea.UserId);

            // Precision for price fields
            modelBuilder.Entity<Event>()
                .Property(e => e.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Ticket>()
                .Property(t => t.Price)
                .HasPrecision(18, 2);
        }
    }
}