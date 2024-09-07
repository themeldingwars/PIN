namespace GameServer.Data.SDB.Records.aptfs;
public record class ResourceNodeBeaconCalldownCommandDef : ICommandDef
{
    public float MaxSlope { get; set; }
    public uint LandedAbility { get; set; }
    public uint CompletedAbility { get; set; }
    public uint Visualrecord { get; set; }
    public uint Calldownfx { get; set; }
    public float MaxRange { get; set; }
    public uint DeathAbility { get; set; }
    public float OutdoorsHeight { get; set; }
    public float Health { get; set; }
    public uint CalldownTimeMs { get; set; }
    public float Radius { get; set; }
    public uint ResourceNodeBeaconId { get; set; }
    public uint Id { get; set; }
    public byte Tier { get; set; }
    public byte GroundAlignment { get; set; }
}
