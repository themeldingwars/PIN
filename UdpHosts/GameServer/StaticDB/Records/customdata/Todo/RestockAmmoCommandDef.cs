namespace GameServer.StaticDB.Records.customdata;

public record RestockAmmoCommandDef : ICommandDef
{
    public uint Id { get; set; }

    // Of the room left in the reserve, null is a full restock
    public uint? Percent { get; set; }
}