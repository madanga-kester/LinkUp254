using LinkUp254.Features.Shared;

namespace LinkUp254.Features.Collections.Models
{
    public class Collection
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public ICollection<CollectionItem> Items { get; set; } = new List<CollectionItem>();
    }
}
