namespace GameServer.Data.SDB.Records.dbitems;
public record class AbilityModule
{
    public uint InitialCooldownSec { get; set; }
    public uint ModuleType { get; set; }
    public uint AbilityChainId { get; set; }
    public uint Id { get; set; }
    public byte ActivatableInPvp { get; set; }
    public byte UiCategory { get; set; }
    public byte ActivatableInPve { get; set; }
    public byte WeaponAltfireMask { get; set; }
    public byte PowerLevel { get; set; }
    public byte ActivatableInAdventure { get; set; }
}