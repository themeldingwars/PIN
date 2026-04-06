namespace GameServer.Data.SDB.Records.dbcharacter;
public record class TinyObject
{
    public float Size { get; set; }
    public uint SpawnPfxId { get; set; }
    public uint PosefileId { get; set; }
    public uint PfxId { get; set; }
    public uint SpawnStatusfxId { get; set; }
    public uint Id { get; set; }
    public uint HitStatusfxId { get; set; }
    public uint Flags { get; set; }
}