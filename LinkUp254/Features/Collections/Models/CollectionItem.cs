using LinkUp254.Features.Events.models;

namespace LinkUp254.Features.Collections.Models
{
    public class CollectionItem
    {
        public int Id { get; set; }
        public string CollectionId { get; set; } = string.Empty;
        public int EventId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public Collection Collection { get; set; } = null!;
        public Event Event { get; set; } = null!;
    }
}
