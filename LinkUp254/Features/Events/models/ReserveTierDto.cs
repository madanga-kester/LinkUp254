using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Events.Models;

public class ReserveTierDto
{
    [Required]
    public int TierId { get; set; }

    [Required]
    [Range(1, 50)]
    public int Quantity { get; set; }
}