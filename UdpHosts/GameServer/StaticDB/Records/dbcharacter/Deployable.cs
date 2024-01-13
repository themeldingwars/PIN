namespace GameServer.Data.SDB.Records.dbcharacter;
public record class Deployable
{
    // public Vec3 VisualOffset { get; set; }
    // public Vec3 AimOffset { get; set; }
    // public Vec3 AimDirection { get; set; }
    public uint LocalizedNameId { get; set; }
    public uint Flags { get; set; }
    public uint CalldownfxCompleteId { get; set; }
    public float ScopeRange { get; set; }
    public uint CollisionId { get; set; }
    public uint ServiceTypeData { get; set; }
    public uint GibsetId { get; set; }
    public string Behavior { get; set; }
    public uint HardpointType { get; set; }
    public long CalldownCompleteSoundevent { get; set; }
    public float XprewardMult { get; set; }
    public uint Visualrec { get; set; }
    public uint BuildTimeMs { get; set; }
    public uint Animnetwork { get; set; }
    public uint CalldownfxId { get; set; }
    public float StandardHealth { get; set; }
    public uint InteractString { get; set; }
    public uint ScalingTableId { get; set; }
    public uint SpawnAbilityid { get; set; }
    public uint Visualgroup { get; set; }
    public uint DeployableLevel { get; set; }
    public uint DialogScriptId { get; set; }
    public uint PoweredOnAbility { get; set; }
    public uint VendorLootTableId { get; set; }
    public uint HitAbilityid { get; set; }
    public uint PoweredOffAbility { get; set; }
    public uint Function { get; set; }
    public uint XprewardType { get; set; }
    public float InteractRadius { get; set; }
    public uint InteractCompletedAbilityid { get; set; }
    public float Scale { get; set; }
    public float InteractHeight { get; set; }
    public uint DeathAbilityid { get; set; }
    public long CalldownSoundevent { get; set; }
    public uint InteractionDurationMs { get; set; }
    public uint ConstructedAbilityid { get; set; }
    public uint InteractAbilityid { get; set; }
    public uint StartHitpoints { get; set; }
    public int GameplayPriorityBoost { get; set; }
    public uint DeployableCategory { get; set; }
    public uint DifficultyCost { get; set; }
    public uint Id { get; set; }
    public uint LootTableId { get; set; }
    public ushort TurretType { get; set; }
    public byte VisualgroupIdx { get; set; }
    public byte ServiceType { get; set; }
    public byte DefaultFaction { get; set; }
    public byte Damageresponse { get; set; }
    public byte InteractionType { get; set; }
    public byte NoDriveZone { get; set; }
}