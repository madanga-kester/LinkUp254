using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Shared;

public class UserInterest
{
    [Key]
    [Column(Order = 1)]
    public int UserId { get; set; }

    [Key]
    [Column(Order = 2)]
    public int InterestId { get; set; }

    public User User { get; set; } = null!;
    public Interest Interest { get; set; } = null!;

    public bool IsActive { get; set; } = true;

   
    public DateTime SelectedAt { get; set; } = DateTime.UtcNow;


}