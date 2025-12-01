using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace LinkUp254.Models
{
    public class LinkUpContext : DbContext
    {
        public LinkUpContext(DbContextOptions<LinkUpContext> options) : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventAtendee> EventAtendees { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
    }
}
