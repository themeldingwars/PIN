namespace GameServer.Data.SDB.Records.aptfs;
public record class DetonateProjectilesCommandDef : ICommandDef
{
    public uint AmmoTypeId { get; set; }
    public uint Id { get; set; }
}
