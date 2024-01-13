namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class CameraLocalPfx
{
    public uint TimeMsBetweenSpawns { get; set; }
    public float SpawnAngleFactor { get; set; }
    public float SpawningRadius { get; set; }
    public uint ParticleAssetId { get; set; }
    public uint Id { get; set; }
    public byte MoveWithCamera { get; set; }
    public byte SpawnOnGround { get; set; }
}
