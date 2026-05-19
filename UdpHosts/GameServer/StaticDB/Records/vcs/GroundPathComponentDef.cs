namespace GameServer.StaticDB.Records.vcs;
public record class GroundPathComponentDef
{
    public float Accel { get; set; }
    public float MaxSpeed { get; set; }
    public uint Id { get; set; }
}