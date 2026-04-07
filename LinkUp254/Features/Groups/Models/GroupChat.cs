using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Groups.Models;

public class GroupChat
{
    [Key]
    public int Id { get; set; }

    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;  

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<GroupMessage> Messages { get; set; } = new List<GroupMessage>();  
}