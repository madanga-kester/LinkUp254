using System.Collections.Generic;

namespace LinkUp254.Features.Shared;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
    public bool HasMore => Offset + Items.Count < Total;
}