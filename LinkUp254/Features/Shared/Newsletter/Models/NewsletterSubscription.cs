using System;
using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Shared.Newsletter.Models
{
    public class NewsletterSubscription : BaseEntity
    {
        [Required]
        public int? UserId { get; set; } 

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        public bool IsConfirmed { get; set; } = true;

        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    }
}