namespace GameServer.Data.SDB.Records.aptfs;
public record class OverrideCollisionCommandDef : ICommandDef
{
    public uint CollisionId { get; set; }
    public uint Posetype { get; set; }
    public uint RagdollId { get; set; }
    public uint Id { get; set; }
}