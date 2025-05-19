namespace GameServer.Data.SDB.Records.dbcharacter;
public record class PoseType
{
    public float PhysicsHeight { get; set; }
    public uint CrouchedCollisionid { get; set; }
    public uint RunningCollisionid { get; set; }
    public uint PoseId { get; set; }
    public uint ProneCollisionid { get; set; }
    public float PhysicsRadius { get; set; }
    public uint FallingCollisionid { get; set; }
    public float PhysicsMass { get; set; }
    public uint StandingCollisionid { get; set; }
    public uint SprintingCollisionid { get; set; }
}