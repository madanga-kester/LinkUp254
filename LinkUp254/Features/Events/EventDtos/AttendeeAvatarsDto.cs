
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace LinkUp254.Features.Events.EventDtos
{


   
    public class AttendeeAvatarsRequest
    {
        [Required]
        public List<int> EventIds { get; set; } = new();
    }

  
    public class AttendeeAvatarsResponse
    {
        public Dictionary<int, List<string>> Avatars { get; set; } = new();

        public Dictionary<int, int> AttendeeCounts { get; set; } = new();
    }
}
