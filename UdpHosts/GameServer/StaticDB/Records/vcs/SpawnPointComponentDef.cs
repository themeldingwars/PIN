namespace GameServer.Data.SDB.Records.vcs;
public record class SpawnPointComponentDef
{
    public uint AutoEjectStatusfxId { get; set; }
    public float SpawnHeightDistanceOffset { get; set; }
    public float SpawnHeightOffset { get; set; }
    public uint Id { get; set; }
    public byte DoNotSeat { get; set; }
    public byte DisableWhenFull { get; set; }
}
