using LinkUp254.Features.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Event
{
    public class EventInterest : BaseEntity
    {
        public int EventId { get; set; }
        public int InterestId { get; set; }

        [ForeignKey("EventId")]
        public Event Event { get; set; } = null!;

        [ForeignKey("InterestId")]
        public Interest Interest { get; set; } = null!;

        public EventInterest() { }

        public EventInterest(int eventId, int interestId) : base()
        {
            EventId = eventId;
            InterestId = interestId;
        }
    }
}
