using LinkUp254.Features.Events.models;
using LinkUp254.Features.Shared;
using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Interests;

public class Interest : BaseEntity
{
    [Key]
    public new int Id { get; set; }  

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Icon { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    
    public new bool IsActive { get; set; } = true;

    
    public ICollection<EventInterest> EventInterests { get; set; } = new List<EventInterest>();
    public ICollection<UserInterest> UserInterests { get; set; } = new List<UserInterest>();
}