namespace GameServer.Data.SDB.Records.aptfs;
public record class MovementTetherCommandDef : ICommandDef
{
    public float ConstRate { get; set; }
    public float MaxVelocity { get; set; }
    public uint MaxRangeRegop { get; set; }
    public float MaxRange { get; set; }
    public float ProportionalRate { get; set; }
    public uint TargetRangeRegop { get; set; }
    public float TargetRange { get; set; }
    public float VelocityDampSecs { get; set; }
    public uint Id { get; set; }
    public byte RateMode { get; set; }
    public byte AllowPrediction { get; set; }
}