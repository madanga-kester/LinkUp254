using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Models
{
    public class Interest : BaseEntity
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Icon { get; set; }  // optional

        public ICollection<UserInterest> UserInterests { get; set; } = new List<UserInterest>();

        public Interest() { }

        public Interest(string name) : base()
        {
            Name = name;
        }
    }
}
