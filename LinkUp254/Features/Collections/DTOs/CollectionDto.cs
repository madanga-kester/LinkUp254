using System.ComponentModel.DataAnnotations;

public class CollectionDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public List<CollectionItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CollectionItemDto
{
    public int EventId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    
    public string? EventTitle { get; set; }
    public DateTime? EventStartTime { get; set; }
}

public class CreateCollectionRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class AddToCollectionRequest
{
    [Required]
    public int EventId { get; set; }
}