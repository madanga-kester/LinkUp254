
using System;
using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Events.DTOs;

public class EventRsvpDto
{
    public int EventId { get; set; }

    [RegularExpression("^(going|interested|none)$")]
    public string Status { get; set; } = "going";

    public DateTime? RsvpedAt { get; set; }

    public int? GuestCount { get; set; } = 1;
    public int? TicketTierId { get; set; }
}

public class RsvpRequest
{
    [RegularExpression("^(going|interested|none)$")]
    public string Status { get; set; } = "going";

    public int? TicketTierId { get; set; }
    public int? GuestCount { get; set; } = 1;
}