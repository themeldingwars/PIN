namespace GameServer.Data.SDB.Records.vcs;
public record class VehicleHardpoints
{
    // public Matrix4x4 Transform { get; set; }
    public uint VisualrecId { get; set; }
    public uint MeshAssetId { get; set; }
    public string Name { get; set; }
}