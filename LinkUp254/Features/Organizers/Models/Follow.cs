using System;
using LinkUp254.Features.Shared;

namespace LinkUp254.Features.Organizers.Models;

public class Follow
{
    public int FollowerId { get; set; }
    public int OrganizerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual User Follower { get; set; } = null!;
    public virtual User Organizer { get; set; } = null!;
}