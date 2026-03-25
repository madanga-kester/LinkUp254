using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Shared
    {
        public class Role : BaseEntity
        {
            [Required]
            public string Name { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            public ICollection<User> Users { get; set; } = new List<User>();

            public Role() { }

            public Role(string name, string description) : base()
            {
                Name = name;
                Description = description;
            }
        }
    }





