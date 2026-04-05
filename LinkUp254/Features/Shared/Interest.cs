using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Shared
{
    public class Interest : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Category { get; set; } = "General";

        public string? Icon { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }

        
        public new bool IsActive { get; set; } = true;

        public ICollection<UserInterest> UserInterests { get; set; } = new List<UserInterest>();
        
        public ICollection<LinkUp254.Features.Events.models.EventInterest> EventInterests { get; set; } = new List<LinkUp254.Features.Events.models.EventInterest>();

        public Interest() { }

        public Interest(string name, string? category = null, string? icon = null) : base()
        {
            Name = name;
            Category = category ?? "General";
            Icon = icon;
        }
    }
}