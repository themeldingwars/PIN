namespace GameServer.Data.SDB.Records.customdata;

public record RestockAmmoCommandDef : ICommandDef
{
    public uint Id { get; set; }
}