namespace GameServer.Data.SDB.Records.vcs;
public record class ScopingComponentDef
{
    public float ScopeRange { get; set; }
    public uint CalldownEffect { get; set; }
    public uint Id { get; set; }
    public float SpawnHeight { get; set; }
    public ushort SpawnAbility { get; set; }
    public ushort DespawnAbility { get; set; }
}
