using System;

namespace WebHost.ClientApi.Armies.Models;

public class PageResults
{
    public uint Page { get; set; }
    public uint TotalCount { get; set; }
    public Array Results { get; set; }
}