namespace GameServer.Data.SDB.Records.dbcharacter;
public record class HitFeedbackEntry
{
    public float DamageAccumulated { get; set; }
    public uint ProbWeight { get; set; }
    public uint ResultType { get; set; }
    public uint ResultId { get; set; }
    public float DamageHealthfrac { get; set; }
    public float HealthFrac { get; set; }
    public uint HitfeedbackId { get; set; }
    public byte DamageCrithit { get; set; }
    public byte DamageType { get; set; }
}
