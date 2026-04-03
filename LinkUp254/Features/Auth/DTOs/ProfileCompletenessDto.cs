namespace LinkUp254.Features.Auth.DTOs
{
    public class ProfileCompletenessResult
    {
        public bool IsComplete { get; set; }
        public int Percentage { get; set; }
        public string? Message { get; set; }
        public List<string>? MissingFields { get; set; }
    }
}
