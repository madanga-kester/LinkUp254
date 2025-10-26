using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Models
{
    public class BaseEntity
    {
        [Key]
        public int Id { get; set; }


        [Required]
        public DateTime CreatedAt { get; set; } 
        public DateTime? UpdatedAt { get; set; }



        [Required]
        public bool IsActive { get; set; } = true;

        


        public BaseEntity()
        {
           CreatedAt= DateTime.UtcNow;

        }



        public BaseEntity(DateTime createdAt, DateTime? updatedAt, bool isActive)
        {
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            IsActive = isActive;
        }



    }
}
