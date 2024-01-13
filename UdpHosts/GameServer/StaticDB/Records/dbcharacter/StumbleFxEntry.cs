namespace GameServer.Data.SDB.Records.dbcharacter;
public record class StumbleFxEntry
{
    public string HardpointName { get; set; }
    public uint PfxAssetId { get; set; }
    public uint StumbleId { get; set; }
}
