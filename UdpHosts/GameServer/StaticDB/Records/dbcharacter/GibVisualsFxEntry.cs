namespace GameServer.Data.SDB.Records.dbcharacter;
public record class GibVisualsFxEntry
{

    public string HardpointName { get; set; }
    public uint GibvisualsId { get; set; }
    public uint PfxAssetId { get; set; }
    public uint Id { get; set; }
}
