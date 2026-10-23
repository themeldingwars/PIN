namespace GameServer.Enums;

/// <summary>
/// The type field of dbcharacter::MonsterVisualOption rows.
/// Values are based on prior manual analysis of the data (see comments).
/// Confidence levels reflect how sure the mapping is.
/// </summary>
public enum MonsterVisualOptionType
{
    /// <summary>Is not used</summary>
    Invalid = 0,

    /// <summary>Confirmed in netcapture, fourth color of static info visuals</summary>
    HairColor = 1,

    /// <summary>Unidentified option, assumed to be a color from its neighbours</summary>
    UnknownColor2 = 2,

    /// <summary>Confirmed in netcapture, fifth color of static info visuals</summary>
    FacialHairColor = 3,

    /// <summary>Confirmed in netcapture, third color of static info visuals</summary>
    LipColor = 4,

    /// <summary>Based on sdb, seems to also be a name, maybe resolved display name?</summary>
    UnknownLocalizationId = 5,

    /// <summary>Based on netcapture, monster 1647</summary>
    NameLocalizationId = 6,

    /// <summary>Assumption based on sdb/netcapture (monster 2612)</summary>
    ChassiWarpaintPaletteId = 7,

    /// <summary>Confirmed in netcapture, second color of static info visuals</summary>
    EyeColor = 8,

    /// <summary>dbcharacter::Head.head_id</summary>
    HeadId = 9,

    /// <summary>dbcharacter::HeadAccessory.ha_id (slot 1)</summary>
    HeadAcc1Id = 10,

    /// <summary>dbcharacter::HeadAccessory.ha_id (slot 2)</summary>
    HeadAcc2Id = 11,

    /// <summary>90% confidence. dbcharacter::VoiceSet.id</summary>
    VoiceSet = 12,

    /// <summary>Unidentified; observed values range from 0 to 467</summary>
    UnknownType13 = 13,

    /// <summary>Unidentified; behaves similarly to type 13</summary>
    UnknownType14 = 14,

    /// <summary>90% confidence. dbcharacter::Monster.chassis_id</summary>
    ChassisId = 15,

    /// <summary>Unidentified. Only 2 parents, 6 entries each; first appears in client db 1783, where it is only on parent
    /// 10003 with values 305071, 305072, 305081 - then these are repeated (maybe M/F templating). Type 17 is the same
    /// with values 305073, 305074, 305075. Two monsters use parent 10003 - 1891 and 1861 - neither has a name entry,
    /// and 1861 is a PeacetimeCityWanderer, which makes sound_event_id a plausible guess</summary>
    UnknownType16 = 16,

    /// <summary>Unidentified; same parents and pattern as type 16</summary>
    UnknownType17 = 17,

    /// <summary>Confirmed in netcapture, first color of static info visuals</summary>
    SkinColor = 18,

    /// <summary>Assuming the order</summary>
    OrnamentsMapGroupId_1 = 19,

    /// <summary>Assuming the order</summary>
    OrnamentsMapGroupId_2 = 20,

    /// <summary>Assuming the order</summary>
    OrnamentsMapGroupId_3 = 21,

    /// <summary>Assuming the order</summary>
    OrnamentsMapGroupId_4 = 22,

    /// <summary>Unidentified. Very few entries, same two parents as type 16/type 17</summary>
    UnknownType23 = 23,

    /// <summary>Unidentified; few entries</summary>
    UnknownType24 = 24,
}
