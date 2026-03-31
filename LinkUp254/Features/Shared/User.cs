using LinkUp254.Features.Events;
using LinkUp254.Features.Group.Messages;

namespace LinkUp254.Features.Shared
{
    public class User : BaseEntity
    {
        public required string FirstName { get; set; } = string.Empty;
        public required string LastName { get; set; } = string.Empty;
        public required string Role { get; set; } = "User";
        public DateTime DateOfBirth { get; set; }
        public required string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public required string Email { get; set; } = string.Empty;
        public required string Password { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }




        public int? Age { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Bio { get; set; }
        public string? Website { get; set; }


       
        

        // Navigations
        public ICollection<Events.Event> EventsHosted { get; set; } = new List<Events.Event>();
        public ICollection<EventAttendee> EventAttendees { get; set; } = new List<EventAttendee>();
        public ICollection<ChatMessage> SentMessages { get; set; } = new List<ChatMessage>();

        // Constructors
        public User() { }

        public User(string firstName, string lastName, string email, string phoneNumber, string password, int age, string city, string country, string state,
            string zipcode, string bio, string webssite)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            Password = password;
            Age = age;
            City = city;
            Country = country;
            State = state;
            ZipCode = zipcode;
            Website = webssite;
            Role = "User";

        }
    }
}