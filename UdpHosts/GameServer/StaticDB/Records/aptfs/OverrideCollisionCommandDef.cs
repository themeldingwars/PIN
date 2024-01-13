namespace GameServer.Data.SDB.Records.aptfs;
public record class OverrideCollisionCommandDef
{
    public uint CollisionId { get; set; }
    public uint Posetype { get; set; }
    public uint RagdollId { get; set; }
    public uint Id { get; set; }
}
