namespace GameServer.Data.SDB.Records.aptfs;
public record class MovementFacingCommandDef
{
    public uint MoveDuration { get; set; }
    public uint MoveDurationRegop { get; set; }
    public uint Id { get; set; }
    public byte AllowPrediction { get; set; }
}
