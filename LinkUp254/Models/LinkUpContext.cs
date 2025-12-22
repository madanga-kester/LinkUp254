using Microsoft.EntityFrameworkCore;

namespace LinkUp254.Models
{
    public class LinkUpContext : DbContext
    {
        public LinkUpContext(DbContextOptions<LinkUpContext> options)
            : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventAtendee> EventAtendees { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<EventAtendee>()
                .HasKey(ea => new { ea.EventId, ea.UserId });

            modelBuilder.Entity<EventAtendee>()
                .HasOne(ea => ea.Event)
                .WithMany(e => e.EventAtendees)
                .HasForeignKey(ea => ea.EventId);

            modelBuilder.Entity<EventAtendee>()
                .HasOne(ea => ea.User)
                .WithMany(u => u.EventAtendee)
                .HasForeignKey(ea => ea.UserId);

            


            modelBuilder.Entity<Event>()
                .Property(e => e.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Ticket>()
                .Property(t => t.Price)
                .HasPrecision(18, 2);
        }
    }
}
