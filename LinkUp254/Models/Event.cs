using System.ComponentModel.DataAnnotations.Schema;


namespace LinkUp254.Models
{
    public class Event : BaseEntity
    {
        
        public string EventId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal? Price { get; set; }
        public string? CoverImage { get; set; }

        public int HostId { get; set; }
       public string HostName { get; set; } = string.Empty;


        [ForeignKey("HostId")]
        public Users Host { get; set; } = null!;



        public ICollection<Users> Attendees { get; set; } = new List<Users>();
    }
}
