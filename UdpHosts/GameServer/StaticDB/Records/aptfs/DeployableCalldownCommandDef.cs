namespace GameServer.Data.SDB.Records.aptfs;
public record class DeployableCalldownCommandDef : ICommandDef
{
    public float ConstructTimeMult { get; set; }
    public float MaxSlope { get; set; }
    public float SametypeRadius { get; set; }
    public uint DeployableType { get; set; }
    public float MaxRange { get; set; }
    public float OutdoorsHeight { get; set; }
    public uint SnapToDeployableType { get; set; }
    public float Radius { get; set; }
    public uint SnapToDeployableError { get; set; }
    public uint Id { get; set; }
    public sbyte MaxOfType { get; set; }
    public byte UseOwnerFaction { get; set; }
    public byte Flip_180 { get; set; }
    public byte GroundAlignment { get; set; }
    public byte PassBonus { get; set; }
    public byte ConstructTimeRegop { get; set; }
    public byte OverrideFactionId { get; set; }
    public byte Thrown { get; set; }
    public byte MaxOfTypeRegop { get; set; }
}
