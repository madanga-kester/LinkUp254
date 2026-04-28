namespace LinkUp254.Features.Events.Models;

public class ReservationResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ReservationId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}