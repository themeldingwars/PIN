namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class SkyboxRecord
{
    public uint CloudNormalMapTextureId { get; set; }
    public uint StratNormalMapTextureId { get; set; }
    public uint CloudDiffuseTextureId { get; set; }
    public uint StratDiffuseTextureId { get; set; }
    public uint StarsTextureId { get; set; }
    public uint Id { get; set; }
}
