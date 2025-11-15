

    using System.ComponentModel.DataAnnotations;

    namespace LinkUp254.Models
    {
        public class Role : BaseEntity
        {
            [Required]
            public string Name { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            public ICollection<Users> Users { get; set; } = new List<Users>();

            public Role() { }

            public Role(string name, string description) : base()
            {
                Name = name;
                Description = description;
            }
        }
    }





