namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireLineOfSightCommandDef
{
    public uint Id { get; set; }
    public byte UseInitPos { get; set; }
    public byte AllowPrediction { get; set; }
}
