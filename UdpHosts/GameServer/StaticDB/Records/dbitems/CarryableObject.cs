namespace GameServer.Data.SDB.Records.dbitems;
public record class CarryableObject
{
    public uint LocalizedNameId { get; set; }
    public uint AbilityGrantedId { get; set; }
    public uint PickupCooldown { get; set; }
    public float ScopeRange { get; set; }
    public uint InteractionTimeMs { get; set; }
    public float PickupRadius { get; set; }
    public uint FirstPickedUpStatusEffect { get; set; }
    public uint ActivateAbilityHostile { get; set; }
    public uint InteractString { get; set; }
    public uint Type { get; set; }
    public float ThrownPickupRadius { get; set; }
    public uint CreatedStatusEffect { get; set; }
    public uint DroppedStatusEffectFriendly { get; set; }
    public uint PickedUpStatusEffectFriendly { get; set; }
    public uint ActivateAbilityFriendly { get; set; }
    public uint VisualGroup { get; set; }
    public uint DroppedStatusEffectHostile { get; set; }
    public uint LocalizedDescriptionId { get; set; }
    public uint PickedUpStatusEffectHostile { get; set; }
    public uint VisualRecordId { get; set; }
    public uint StoppedMovingStatusEffect { get; set; }
    public uint Id { get; set; }
    public byte AllowFriendlyPickup { get; set; }
    public byte VisualGroupIdx { get; set; }
    public byte ShouldTrackForPvpScoreboard { get; set; }
    public byte PickupByInteraction { get; set; }
    public byte MaxPerCharacter { get; set; }
    public byte IsExclusive { get; set; }
    public byte AllowVehicleCarry { get; set; }
    public byte AllowHostilePickup { get; set; }
}