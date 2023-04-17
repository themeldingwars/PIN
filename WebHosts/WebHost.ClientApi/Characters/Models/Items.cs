using System.Collections.Generic;

namespace WebHost.ClientApi.Characters.Models;

public class Items
{
    public ulong ItemId { get; set; }
    public uint ItemSdbId { get; set; }
    public ulong OwnerGuid { get; set; }
    public uint TypeCode { get; set; }
    public uint Quality { get; set; }
    public ulong CharacterGuid { get; set; }
    public bool BoundToOwner { get; set; }
    public string CreatedAt { get; set; }
    public string UpdatedAt { get; set; }
    public Durability Durability { get; set; }
    public Dictionary<uint, double> AttributeModifiers { get; set; }
    public ulong CreatorGuid { get; set; }
}