namespace GameServer.Data.SDB.Records.dbcharacter;
public record class TinyObject
{
    public float Size { get; set; }
    public int SpawnPfxId { get; set; }
    public int PosefileId { get; set; }
    public int PfxId { get; set; }
    public int SpawnStatusfxId { get; set; }
    public int Id { get; set; }
    public int HitStatusfxId { get; set; }
    public int Flags { get; set; }
}