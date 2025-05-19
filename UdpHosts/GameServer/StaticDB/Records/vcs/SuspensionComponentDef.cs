namespace GameServer.Data.SDB.Records.vcs;
public record class SuspensionComponentDef
{
    public float Length3 { get; set; }
    public string AttachmentHardpointPrefix { get; set; }
    public uint LengthMap1 { get; set; }
    public float DampingRelaxation3 { get; set; }
    public uint LengthMap2 { get; set; }
    public float DampingRelaxation1 { get; set; }
    public float Length2 { get; set; }
    public float DampingRelaxation2 { get; set; }
    public uint LengthMap3 { get; set; }
    public float Length1 { get; set; }
    public float DampingCompression1 { get; set; }
    public float Strength1 { get; set; }
    public float Strength3 { get; set; }
    public string PivotHardpointPrefix { get; set; }
    public float DampingCompression3 { get; set; }
    public float Strength2 { get; set; }
    public float DampingCompression2 { get; set; }
    public uint Id { get; set; }
    public string MaxextentHardpointPrefix { get; set; }
    public byte LeadingZero { get; set; }
    public byte ZeroBased { get; set; }
}