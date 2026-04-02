using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Shared
{
  
    [Index(nameof(UserId), nameof(InterestId), IsUnique = true)]
    public class UserInterest : BaseEntity
    {
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        public int InterestId { get; set; }

        [ForeignKey("InterestId")]
        public Interest Interest { get; set; } = null!;

        public DateTime SelectedAt { get; set; } = DateTime.UtcNow;

       
        public new bool IsActive { get; set; } = true;

     

        public UserInterest() { }

        public UserInterest(int userId, int interestId) : base()
        {
            UserId = userId;
            InterestId = interestId;
            SelectedAt = DateTime.UtcNow;
        }
    }
}