namespace GameServer.StaticDB.Records.customdata;

public record RestockAmmoCommandDef : ICommandDef
{
    public uint Id { get; set; }
}