namespace GameServer.Data.SDB.Records.dbdecals;
public record class DecalRecord
{
    public float ScaleY { get; set; }
    public int MaterialId { get; set; }
    public int MaxInstances { get; set; }
    public float Lifetime { get; set; }
    public float RandomScaleMin { get; set; }
    public float RandomScaleMax { get; set; }
    public float ScaleX { get; set; }
    public int GrowIntoId { get; set; }
    public int Texture2Id { get; set; }
    public int GrowIntoCount { get; set; }
    public float FadeDuration { get; set; }
    public float ScaleZ { get; set; }
    public int GrowIntoPfxId { get; set; }
    public float GlowTime { get; set; }
    public int[] Vertices { get; set; }
    public int Texture0Id { get; set; }
    public int[] GlowGradient { get; set; }
    public int Texture1Id { get; set; }
    public int Id { get; set; }
    public int NumTilesX { get; set; }
    public int QualityFlags { get; set; }
    public int RandomRotation { get; set; }
    public int NumTilesY { get; set; }
}
