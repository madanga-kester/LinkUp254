//grouping model for social media application according to their interests or private meetups


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace LinkUp254.Models
{
    public class Group: BaseEntity
    {

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Description { get; set; }
        public string? CoverImage { get; set; }


        // The user who created the group
        [ForeignKey("CreatedBy")]
        public int CreatedById { get; set; }
        public Users? CreatedBy { get; set; }



        public ICollection<Users> Members { get; set; } = new List<Users>();
         public ICollection<>
    }
}
