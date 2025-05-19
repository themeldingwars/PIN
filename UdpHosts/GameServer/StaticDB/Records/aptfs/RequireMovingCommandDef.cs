namespace GameServer.Data.SDB.Records.aptfs;
public record class RequireMovingCommandDef : ICommandDef
{
    public float Velocitytol { get; set; }
    public uint Id { get; set; }
    public byte CheckVelocity { get; set; }
    public byte CheckAiming { get; set; }
    public byte Negate { get; set; }
    public byte AllowPrediction { get; set; }
}