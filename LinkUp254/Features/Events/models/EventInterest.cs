using LinkUp254.Features.Shared; 

namespace LinkUp254.Features.Events.models;

public class EventInterest
{
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public int InterestId { get; set; }
    public Interest Interest { get; set; } = null!;  

    public float Weight { get; set; } = 1f;
}