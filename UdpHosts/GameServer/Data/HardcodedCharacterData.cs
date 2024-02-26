using System;
using System.Collections.Generic;
using System.Linq;
using AeroMessages.GSS.V66.Character;
using AeroMessages.GSS.V66.Character.Event;

namespace GameServer.Data;

public static class HardcodedCharacterData
{
    public static string ArmyTag = "[ARMY]";
    public static ulong ArmyGUID = 1u;
    public static uint SelectedLoadout = 184538131;
    public static byte Level = 45;
    public static byte EffectiveLevel = 45;
    public static uint MaxHealth = 19192;

    public static BasicCharacterData FallbackData = new BasicCharacterData()
    {
        CharacterInfo = new BasicCharacterInfo()
        {
            Name = "Fallback",
            Gender = (uint)CharacterGender.Male,
            Race = (uint)CharacterRace.Human,
            TitleId = 135,
            CurrentBattleframeSDBId = 76331,
        },
        CharacterVisuals = new BasicCharacterVisuals()
        {
            Head = 10002,
            Eyes = 0,
            VoiceSet = 1000,
            Vehicle = 1000,
            Glider = 1000,
            HeadAccessories = new uint[2] { 10089, 10106 },
            Ornaments = new uint[] { 10224, 10270, 10061 },

            SkinColor = 0x52680000u,
            EyeColor = 0x6a2440e0u,
            LipColor = 0xffff0000u,
            HairColor = 0x320D0021u,
            FacialHairColor = 0x320D0021u,
        }
    };

    public static Dictionary<uint, uint> TempAvailableLoadouts = new Dictionary<uint, uint>()
    {
        // Accord
        { 1, 75772 }, // Dreadnaught
        { 2, 76164 }, // Assault
        { 3, 75774 }, // Biotech
        { 5, 75775 }, // Engineer
        { 7, 75773 }, // Recon

        // Advanced
        { 28, 76133 }, // Firecat
        { 20, 76132 }, // Tigerclaw
        { 30, 76337 }, // Electron
        { 22, 76338 }, // Bastion
        { 184538131, 76331 }, // Mammoth // 23
        { 31, 76332 }, // Rhino
        { 29, 76335 }, // Dragonfly
        { 21, 76336 }, // Recluse
        { 24, 76333 }, // Nighthawk
        { 32, 76334 }, // Raptor

        // Advanced 2
        { 47, 82359 }, // Graviton
        { 48, 82360 }, // Arsenal
        { 49, 82394 }, // Archangel

        // Social
        { 246, 124356 }, // Beach Party
        { 247, 77733 }, // BattleLab Trainee
    };

    public static uint[] FallbackInventoryItems =
    [
        151,
        33800,
        56728,
        56811,
        75257,
        75772,
        75773,
        75774,
        75775,
        75820,
        76132,
        76133,
        76164,
        76331,
        76332,
        76333,
        76334,
        76335,
        76336,
        76337,
        76338,
        76958,
        76999,
        77018,
        77019,
        77066,
        77067,
        77068,
        77087,
        77340,
        77367,
        77402,
        77496,
        77504,
        77510,
        77523,
        81352,
        81361,
        81362,
        81423,
        81457,
        81490,
        82337,
        82360,
        84820,
        85228,
        85511,
        85555,
        85958,
        86400,
        87058,
        87483,
        87542,
        87779,
        87848,
        87899,
        90519,
        90623,
        96464,
        96487,
        96578,
        96946,
        98188,
        99093,
        101940,
        101995,
        102046,
        102427,
        103986,
        104170,
        104353,
        105495,
        105810,
        105920,
        105981,
        106103,
        106785,
        107730,
        107784,
        110768,
        113514,
        113520,
        113552,
        113556,
        113687,
        113712,
        113718,
        113742,
        113810,
        113838,
        113853,
        113858,
        113873,
        113931,
        113972,
        113996,
        114008,
        114026,
        114044,
        114050,
        114068,
        114074,
        114088,
        114094,
        114112,
        114142,
        114160,
        114166,
        114226,
        114260,
        114316,
        114320,
        118133,
        118733,
        120125,
        124132,
        124140,
        124142,
        124651,
        124665,
        124671,
        125435,
        125445,
        125483,
        125631,
        125636,
        125683,
        125725,
        125845,
        126127,
        126165,
        126309,
        126413,
        126500,
        126529,
        126534,
        126539,
        126575,
        126896,
        127030,
        127040,
        127097,
        127134,
        127144,
        127223,
        127270,
        127306,
        127626,
        127636,
        127770,
        127812,
        127864,
        127874,
        127953,
        128000,
        128036,
        128356,
        128361,
        128500,
        128552,
        128557,
        128604,
        128688,
        128689,
        128730,
        128766,
        128958,
        128990,
        129010,
        129020,
        129056,
        129061,
        129202,
        129242,
        129312,
        129396,
        129458,
        129604,
        129993,
        130000,
        130370,
        130419,
        130436,
        130595,
        130596,
        130597,
        130598,
        130599,
        130600,
        130602,
        130606,
        131708,
        132068,
        132110,
        132442,
        132620,
        134616,
        136056,
        136514,
        136576,
        136660,
        136722,
        137212,
        138462,
        138795,
        139124,
        139125,
        139303,
        139665,
        139736,
        139807,
        139875,
        139922,
        140705,
        140755,
        140757,
        141873,
        141906,
        141929,
        142078,
        142119,
        142132,
        143123,
        143125,
        143131,
        143132,
        143133,
        143134,
        143195,
        143211,
        143212,
        143250,
        143330,
        143333,
        143368,
        143372,
        143380,
        143437,
        143670,
    ];

    public static (uint, uint)[] FallbackInventoryResourcesShort =
    [
        (10, 3531909),
        (56, 11),
        (30101, 98780),
        (30298, 1268),
        (30412, 4),
        (32755, 21),
        (34107, 5824641),
    ];

    public static (uint, uint)[] FallbackInventoryResources =
    [
        (10, 3531909),
        (56, 11),
        (30101, 98780),
        (30298, 1268),
        (30412, 4),
        (32755, 21),
        (34107, 5824641),
        (54003, 4467),
        (56835, 76),
        (75425, 460),
        (76947, 10),
        (76978, 1),
        (76979, 1),
        (76984, 56),
        (76985, 60),
        (76986, 30),
        (77014, 10),
        (77015, 8),
        (77132, 3330),
        (77205, 315),
        (77343, 14),
        (77344, 1),
        (77345, 6),
        (77346, 14),
        (77403, 7),
        (77404, 6),
        (77428, 334),
        (77429, 330),
        (77430, 3),
        (77606, 3),
        (77607, 2),
        (77643, 16),
        (77682, 1),
        (77860, 2),
        (78032, 2),
        (80274, 2),
        (81487, 1),
        (82577, 547),
        (82595, 108),
        (82596, 128),
        (82597, 15),
        (82598, 174),
        (82604, 20),
        (82624, 45),
        (82737, 1),
        (85412, 1),
        (85415, 1),
        (85416, 1),
        (85422, 2),
        (85454, 1),
        (85535, 21),
        (85679, 7),
        (85792, 1),
        (85824, 5),
        (86004, 1),
        (86013, 2),
        (86154, 9123363),
        (86360, 1),
        (86401, 9),
        (86498, 2),
        (86526, 1),
        (86621, 11814),
        (86628, 7),
        (86667, 4978),
        (86668, 1560),
        (86669, 1487),
        (86672, 239),
        (86673, 608),
        (86679, 1344),
        (86681, 13),
        (86682, 1),
        (86683, 702),
        (86696, 12),
        (86699, 5),
        (86703, 10750),
        (86709, 903),
        (86711, 429),
        (86713, 2140),
        (95083, 9),
        (95084, 33),
        (95085, 17),
        (95086, 2),
        (95087, 7),
        (95088, 35),
        (95089, 9),
        (95093, 6),
        (95094, 5),
        (95095, 3),
        (95096, 5),
        (95097, 26),
        (95098, 22),
        (95099, 20),
        (116563, 36),
        (117036, 5),
        (117702, 9),
        (118147, 5),
        (118355, 54),
        (120498, 43),
        (120622, 1),
        (120974, 14),
        (121093, 6),
        (121377, 1),
        (122951, 15),
        (122953, 1),
        (122954, 1),
        (123027, 19),
        (123052, 21),
        (123217, 1),
        (123353, 2),
        (123384, 1),
        (123400, 1),
        (123404, 1),
        (123426, 1),
        (124249, 2814),
        (124250, 3323),
        (124251, 5296),
        (124334, 10),
        (124565, 11),
        (124624, 14),
        (124626, 29),
        (124652, 1),
        (124653, 5),
        (124654, 1),
        (136476, 2),
        (138302, 30),
        (138334, 3),
        (138694, 2),
        (138695, 7),
        (138754, 1),
        (138755, 1),
        (139683, 5651),
        (139960, 5),
        (139961, 12),
        (140057, 100),
        (140737, 1),
        (140743, 2),
        (142144, 13087),
        (142185, 1),
        (142268, 18),
        (143124, 2),
        (143677, 60),
    ];

    public static uint LookupTempAvailableLoadoutId(uint chassisId) => TempAvailableLoadouts
    .Where((pair) => pair.Value == chassisId)
    .Select((pair) => pair.Key)
    .FirstOrDefault(HardcodedCharacterData.SelectedLoadout);

    public static AeroMessages.GSS.V66.Character.Event.Loadout[] GetTempAvailableLoadouts()
    {
        var entries = new List<Loadout>();
        foreach (var pair in TempAvailableLoadouts)
        {
            entries.Add(new Loadout()
            {
                FrameLoadoutId = pair.Key,
                ChassisID = pair.Value,
                LoadoutName = $"Loadout {pair.Key}",
                LoadoutType = "battleframe",
                LoadoutConfigs = new LoadoutConfig[]
                {
                    new()
                    {
                        ConfigID = 0,
                        ConfigName = "pve",
                        Items = Array.Empty<LoadoutConfig_Item>(),
                        Visuals = Array.Empty<LoadoutConfig_Visual>(),
                        Perks = Array.Empty<uint>(),
                        Unk1 = 0,
                        PerkBandwidth = 0,
                        PerkRespecLockRemainingSeconds = 0,
                        HaveExtraData = 0
                    },
                    new()
                    {
                        ConfigID = 1,
                        ConfigName = "pvp",
                        Items = Array.Empty<LoadoutConfig_Item>(),
                        Visuals = Array.Empty<LoadoutConfig_Visual>(),
                        Perks = Array.Empty<uint>(),
                        Unk1 = 0,
                        PerkBandwidth = 0,
                        PerkRespecLockRemainingSeconds = 0,
                        HaveExtraData = 0
                    }
                }
            });
        }

        return entries.ToArray();
    }
}

public class BasicCharacterInfo
{
    public string Name { get; set; }
    public uint Gender { get; set; }
    public uint Race { get; set; }
    public ushort TitleId { get; set; }
    public uint CurrentBattleframeSDBId { get; set; }
}

public class BasicCharacterVisuals
{
    public uint Head { get; set; }
    public uint Eyes { get; set; }
    public uint VoiceSet { get; set; }
    public uint Vehicle { get; set; }
    public uint Glider { get; set; }
    public uint[] HeadAccessories { get; set; }
    public uint[] Ornaments { get; set; }
    public uint SkinColor { get; set; }
    public uint EyeColor { get; set; }
    public uint LipColor { get; set; }
    public uint HairColor { get; set; }
    public uint FacialHairColor { get; set; }
}

public class BasicCharacterData
{
    public BasicCharacterInfo CharacterInfo { get; set; }
    public BasicCharacterVisuals CharacterVisuals { get; set; }
}