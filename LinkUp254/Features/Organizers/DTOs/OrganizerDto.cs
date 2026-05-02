namespace LinkUp254.Features.Organizers.DTOs;

public class OrganizerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
    public int JoinedYear { get; set; }
    public int FollowerCount { get; set; }
    public int EventCount { get; set; }
    public double AverageRating { get; set; }
    public bool IsVerified { get; set; }
}