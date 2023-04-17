namespace WebHost.ClientApi.Characters.Models;

public class Resources
{
    public uint ItemSdbId { get; set; }
    public ulong OwnerGuid { get; set; }
    public string ResourceType { get; set; }
    public uint Quantity { get; set; }
}