using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Models
{
    public class Report : BaseEntity
    {
        public int ReporterId { get; set; }

        [Required]
        public string TargetType { get; set; } = string.Empty;
        // Post, Comment, User

        public int TargetId { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;

        [ForeignKey(nameof(ReporterId))]
        public Users Reporter { get; set; } = null!;
    }
}
