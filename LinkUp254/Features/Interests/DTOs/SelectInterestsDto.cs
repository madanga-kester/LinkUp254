using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Interests.DTOs
{
    public class SelectInterestsDto
    {
        [Required]
        public List<int> InterestIds { get; set; } = new();
    }
}