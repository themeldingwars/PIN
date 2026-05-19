namespace GameServer.StaticDB.Records.aptfs;
public record class ApplyAmmoRiderCommandDef : ICommandDef
{
    public uint AmmoId { get; set; }
    public uint Id { get; set; }
}