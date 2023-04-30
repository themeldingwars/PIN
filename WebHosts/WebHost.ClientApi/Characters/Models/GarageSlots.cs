using System;

namespace WebHost.ClientApi.Characters.Models;

public class GarageSlots
{
    public uint Id { get; set; }
    public string Name { get; set; }
    public ulong CharacterGuid { get; set; }
    public string GarageType { get; set; }
    public ulong ItemGuid { get; set; }
    public Array EquippedSlots { get; set; }
    public ItemLimits Limits { get; set; }
    public Array Decals { get; set; }
    public uint VisualLoadoutId { get; set; }
    public uint WarpaintId { get; set; }
    public Array Warpaintpatterns { get; set; }
    public Array VisualOverrides { get; set; }
    public bool Unlocked { get; set; }
    public uint ExpiresInSecs { get; set; }
}

public class Equippedslots
{
    public uint SlotTypeId { get; set; }
    public uint SdbId { get; set; }
    public ulong ItemGuid { get; set; }
    public uint AllocatedPower { get; set; }
    public string Unlocked { get; set; }
}

public class Decals
{
    public uint SdbId { get; set; }
    public uint Color { get; set; }
    public Array Transform { get; set; }
}

public class ItemLimits
{
    public uint Abilities { get; set; }
}

public class Warpaintpatterns
{
    public uint SdbId { get; set; }
    public Array Transform { get; set; }
    public uint Usage { get; set; }
}

public class GarageSlotPerks
{
    public Array Perks { get; set; }
    public uint Respecs { get; set; }
    public uint MaxPoints { get; set; }
}