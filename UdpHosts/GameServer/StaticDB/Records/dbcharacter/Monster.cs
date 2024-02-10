namespace GameServer.Data.SDB.Records.dbcharacter;
public record class Monster
{
    // public Vec4f extra_color_3 { get; set; }
    // public Vec4f extra_color_4 { get; set; }
    // public Vec4f extra_color_1 { get; set; }
    // public Vec4f extra_color_2 { get; set; }
    // public Vec3f projectile_offset { get; set; }
    public uint LocalizedNameId { get; set; }
    public uint BackpackId { get; set; }
    public uint OrnamentsMapGroupId_3 { get; set; }
    public float FastSpeed { get; set; }
    public uint XpResourceId { get; set; }
    public uint HairColor { get; set; }
    public uint HeadAcc2Id { get; set; }
    public float NormalSpeed { get; set; }
    public uint EyeColor { get; set; }
    public uint BodysuitWarpaintPaletteId { get; set; }
    public float BodyRadius { get; set; }
    public string Behavior { get; set; }
    public uint LipColor { get; set; }
    public uint FullbodyWarpaintPaletteId { get; set; }
    public uint OrnamentsMapGroupId_4 { get; set; }
    public uint OrnamentsMapGroupId_1 { get; set; }

    // public int[] WarpaintTattooData { get; set; }
    public uint Weapon1Id { get; set; }
    public uint ChassisId { get; set; }
    public uint GlowWarpaintPaletteId { get; set; }
    public uint BehaviorDefensiveInstanceId { get; set; }
    public uint PosetypeId { get; set; }
    public uint SkinColor { get; set; }

    // public int[] WarpaintPatternData { get; set; }
    public uint ScalingTableId { get; set; }
    public float MinRandScale { get; set; }
    public uint VisualOptionsId { get; set; }
    public float HealthRegen { get; set; }
    public uint LootTable2Id { get; set; }
    public uint FactionId { get; set; }
    public uint Weapon2Id { get; set; }
    public uint HeadId { get; set; }
    public float BodyMass { get; set; }
    public uint XprewardType { get; set; }
    public uint BehaviorOffensiveInstanceId { get; set; }
    public uint VisualsGroupId { get; set; }
    public uint AiSpawnDelayMs { get; set; }
    public string TerminalTypeName { get; set; }
    public string BehaviorOffensive { get; set; }
    public uint EyesId { get; set; }
    public string BehaviorDefensive { get; set; }
    public uint BehaviorInstanceId { get; set; }
    public uint HeadAcc1Id { get; set; }
    public uint CharinfoId { get; set; }
    public uint CraftingTypeId { get; set; }
    public float NetworkFidelity { get; set; }
    public uint FacialHairColor { get; set; }
    public float MaxRandScale { get; set; }
    public uint OrnamentsMapGroupId_2 { get; set; }
    public uint VendorId { get; set; }
    public uint ArmorWarpaintPaletteId { get; set; }
    public uint DifficultyCost { get; set; }
    public uint Id { get; set; }
    public float BodyHeight { get; set; }
    public uint LootTableId { get; set; }
    public uint VoiceSet { get; set; }
    public ushort Title { get; set; }
    public ushort RespawnFlags { get; set; }
    public byte Gravity { get; set; }
    public byte IsComponented { get; set; }
    public byte DamageResponseId { get; set; }
    public char Gender { get; set; }
    public byte Race { get; set; }
}