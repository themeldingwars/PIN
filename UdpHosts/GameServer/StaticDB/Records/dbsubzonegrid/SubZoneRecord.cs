namespace GameServer.Data.SDB.Records.dbsubzonegrid;
public record class SubZoneRecord
{
    public uint? CameraLocalPfxId1 { get; set; }
    public uint? ColorGradingRecordId { get; set; }
    public uint? MusicSwitchId { get; set; }
    public uint? CameraLocalPfxId3 { get; set; }
    public uint? CameraLocalPfxId2 { get; set; }
    public int[] EnvironmentData { get; set; }
    public uint DisplayNameId { get; set; }
    public uint? SoundEventId { get; set; }
    public uint Id { get; set; }
    public uint? SkyboxRecordId { get; set; }
    public byte HasResources { get; set; }
    public byte ResourceTableIdOverrides { get; set; }
}