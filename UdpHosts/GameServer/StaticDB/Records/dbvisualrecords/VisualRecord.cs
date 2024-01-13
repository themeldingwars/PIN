namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class VisualRecord
{
    public uint RagdollCollisionId { get; set; }
    public float LodDistanceMultiplier { get; set; }
    public uint HitboxCollisionId { get; set; }
    public uint Flags { get; set; }
    public uint Id { get; set; }
}
