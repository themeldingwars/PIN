namespace GameServer.Data.SDB.Records.dbphysicsmaterials;
public record class PhysicsMaterial
{
    public float AIPathingCost { get; set; }
    public uint AudioVisualMaterialId { get; set; }
    public float SoundPropagation { get; set; }
    public string Name { get; set; }
    public uint Id { get; set; }
    public float Friction { get; set; }
    public byte IsRoad { get; set; }
    public byte IsCritHit { get; set; }
    public byte IsTerrainic { get; set; }
}