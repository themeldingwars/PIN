namespace GameServer.Data.SDB.Records.aptfs;
public record class ApplyImpulseCommandDef : ICommandDef
{
    public float Loftangle { get; set; }
    public uint Duration { get; set; }
    public float Yawangle { get; set; }
    public float Speed { get; set; }
    public uint Id { get; set; }
    public byte Alongvelocity { get; set; }
    public byte SpeedRegop { get; set; }
    public byte AllowPrediction { get; set; }
}