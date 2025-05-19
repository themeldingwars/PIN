namespace GameServer.Data.SDB.Records.dbcharacter;
public record class GibVisualsPropEntry
{
    // public Vec3 DirectionBias { get; set; }
    public string RagdollboneRemove { get; set; }
    public uint VisualrecId { get; set; }
    public float MinSpeed { get; set; }
    public string HardpointName { get; set; }
    public uint GibvisualsId { get; set; }
    public float MaxSpeed { get; set; }
    public uint PfxAssetId { get; set; }
    public uint Id { get; set; }
}