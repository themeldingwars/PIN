namespace GameServer.Data.SDB.Records.aptfs;
public record class RegisterClientProximityCommandDef : ICommandDef
{
    public uint Chain { get; set; }
    public uint AbilityId { get; set; }
    public uint MaxTargets { get; set; }
    public uint RetryInterval { get; set; }
    public float Radius { get; set; }
    public uint Id { get; set; }
    public byte RadiusRegop { get; set; }
}
