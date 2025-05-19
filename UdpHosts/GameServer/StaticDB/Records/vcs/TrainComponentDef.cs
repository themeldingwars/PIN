namespace GameServer.Data.SDB.Records.vcs;
public record class TrainComponentDef
{
    public float BobAmplitude { get; set; }
    public uint VisaulrecId { get; set; }
    public float BobFrequency { get; set; }
    public uint NumCars { get; set; }
    public uint Id { get; set; }
}