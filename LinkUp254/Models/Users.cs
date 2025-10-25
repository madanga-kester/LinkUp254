using Microsoft.Extensions.Logging;

namespace LinkUp254.Models
{
    public class Users : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }



        public ICollection<Event> EventsHosted { get; set; } = new List<Event>();

        
        public ICollection<Event> EventsJoined { get; set; } = new List<Event>();

    }    
}
