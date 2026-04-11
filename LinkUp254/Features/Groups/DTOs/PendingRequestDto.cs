
using System;

namespace LinkUp254.Features.Groups.DTOs;

public class PendingRequestDto
{
    public int Id { get; set; }
    public UserDto User { get; set; } = new UserDto(); 
    public string? Message { get; set; }
    public DateTime RequestedAt { get; set; }
}