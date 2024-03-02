using System;
using System.Collections.Generic;
using System.Linq;
using AeroMessages.GSS.V66.Character;
using AeroMessages.GSS.V66.Character.Event;
using GameServer.Data.SDB;
using GameServer.Data.SDB.Records.dbcharacter;

namespace GameServer.Data;

public static class HardcodedCharacterData
{
    public static string ArmyTag = "[ARMY]";
    public static ulong ArmyGUID = 1u;
    public static uint SelectedLoadout = 184538131;
    public static byte Level = 45;
    public static byte EffectiveLevel = 45;
    public static uint MaxHealth = 19192;
    public static uint GeneratedLoadoutCounter = 20001;

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

    public static Dictionary<uint, uint> TempCharCreateLoadouts = new Dictionary<uint, uint>()
    {
        // Accord
        { 287, 75772 }, // Dreadnaught
        { 286, 76164 }, // Assault
        { 288, 75774 }, // Biotech
        { 289, 75775 }, // Engineer
        { 290, 75773 }, // Recon

        // Advanced
        { 299, 76133 }, // Firecat
        { 300, 76132 }, // Tigerclaw
        { 295, 76337 }, // Electron
        { 296, 76338 }, // Bastion
        { 294, 76331 }, // Mammoth
        { 293, 76332 }, // Rhino
        { 297, 76335 }, // Dragonfly
        { 298, 76336 }, // Recluse
        { 291, 76333 }, // Nighthawk
        { 292, 76334 }, // Raptor

        // Advanced 2
        { 47, 82359 }, // Graviton
        { 48, 82360 }, // Arsenal
        { 49, 82394 }, // Archangel

        // Social
        { 246, 124356 }, // Beach Party
        { 247, 77733 }, // BattleLab Trainee
    };

    public static List<LoadoutReferenceData> TempHardcodedLoadouts = 
    [
        /*
        // Accord
        new LoadoutReferenceData
        {
            ChassisId = 75772, // Dreadnaught
            SlottedItemsPvE = new Dictionary<LoadoutSlotType, uint>()
            {
                { LoadoutSlotType.Primary, 86851 },
                { LoadoutSlotType.Secondary, 87800 },
                { LoadoutSlotType.AbilityHKM, 125199 },
                { LoadoutSlotType.Ability1, 90493 },
                { LoadoutSlotType.Ability2, 0 },
                { LoadoutSlotType.Ability3, 0 },
                { LoadoutSlotType.Backpack, 75873 },
                { LoadoutSlotType.GearTorso, 126000 },
                { LoadoutSlotType.GearAuxWeapon, 129505 },
                { LoadoutSlotType.GearMedicalSystem, 0 },
                { LoadoutSlotType.GearHead, 0 },
                { LoadoutSlotType.GearArms, 127501 },
                { LoadoutSlotType.GearLegs, 128271 },
                { LoadoutSlotType.GearReactor, 126731 },
                { LoadoutSlotType.GearOS, 129067 },
                { LoadoutSlotType.GearGadget1, 0 },
                { LoadoutSlotType.GearGadget2, 0 },
            }
        },
        new LoadoutReferenceData
        {
            ChassisId = 76164, // Assault
            SlottedItemsPvE = new Dictionary<LoadoutSlotType, uint>()
            {
                { LoadoutSlotType.Primary, 86742 },
                { LoadoutSlotType.Secondary, 87741 },
                { LoadoutSlotType.AbilityHKM, 88491 },
                { LoadoutSlotType.Ability1, 88039 },
                { LoadoutSlotType.Ability2, 0 },
                { LoadoutSlotType.Ability3, 0 },
                { LoadoutSlotType.Backpack, 75877 },
                { LoadoutSlotType.GearTorso, 126000 },
                { LoadoutSlotType.GearAuxWeapon, 129213 },
                { LoadoutSlotType.GearMedicalSystem, 0 },
                { LoadoutSlotType.GearHead, 0 },
                { LoadoutSlotType.GearArms, 127501 },
                { LoadoutSlotType.GearLegs, 128271 },
                { LoadoutSlotType.GearReactor, 126731 },
                { LoadoutSlotType.GearOS, 129067 },
                { LoadoutSlotType.GearGadget1, 0 },
                { LoadoutSlotType.GearGadget2, 0 },
            }
        },
        new LoadoutReferenceData
        {
            ChassisId = 75774, // Biotech
            SlottedItemsPvE = new Dictionary<LoadoutSlotType, uint>()
            {
                { LoadoutSlotType.Primary, 87028 },
                { LoadoutSlotType.Secondary, 87918 },
                { LoadoutSlotType.AbilityHKM, 89124 },
                { LoadoutSlotType.Ability1, 123271 },
                { LoadoutSlotType.Ability2, 0 },
                { LoadoutSlotType.Ability3, 0 },
                { LoadoutSlotType.Backpack, 75874 },
                { LoadoutSlotType.GearTorso, 126000 },
                { LoadoutSlotType.GearAuxWeapon, 129213 },
                { LoadoutSlotType.GearMedicalSystem, 0 },
                { LoadoutSlotType.GearHead, 0 },
                { LoadoutSlotType.GearArms, 127501 },
                { LoadoutSlotType.GearLegs, 128271 },
                { LoadoutSlotType.GearReactor, 126731 },
                { LoadoutSlotType.GearOS, 129067 },
                { LoadoutSlotType.GearGadget1, 0 },
                { LoadoutSlotType.GearGadget2, 0 },
            }
        },
        new LoadoutReferenceData
        {
            ChassisId = 75775, // Engineer
            SlottedItemsPvE = new Dictionary<LoadoutSlotType, uint>()
            {
                { LoadoutSlotType.Primary, 87386 },
                { LoadoutSlotType.Secondary, 87978 },
                { LoadoutSlotType.AbilityHKM, 91559 },
                { LoadoutSlotType.Ability1, 91394 },
                { LoadoutSlotType.Ability2, 0 },
                { LoadoutSlotType.Ability3, 0 },
                { LoadoutSlotType.Backpack, 31344 },
                { LoadoutSlotType.GearTorso, 126000 },
                { LoadoutSlotType.GearAuxWeapon, 129359 },
                { LoadoutSlotType.GearMedicalSystem, 0 },
                { LoadoutSlotType.GearHead, 0 },
                { LoadoutSlotType.GearArms, 127501 },
                { LoadoutSlotType.GearLegs, 128271 },
                { LoadoutSlotType.GearReactor, 126731 },
                { LoadoutSlotType.GearOS, 129067 },
                { LoadoutSlotType.GearGadget1, 0 },
                { LoadoutSlotType.GearGadget2, 0 },
            }
        },
        new LoadoutReferenceData
        {
            ChassisId = 75773, // Recon
            SlottedItemsPvE = new Dictionary<LoadoutSlotType, uint>()
            {
                { LoadoutSlotType.Primary, 86969 },
                { LoadoutSlotType.Secondary, 87918 },
                { LoadoutSlotType.AbilityHKM, 91770 },
                { LoadoutSlotType.Ability1, 91662 },
                { LoadoutSlotType.Ability2, 0 },
                { LoadoutSlotType.Ability3, 0 },
                { LoadoutSlotType.Backpack, 75876 },
                { LoadoutSlotType.GearTorso, 126000 },
                { LoadoutSlotType.GearAuxWeapon, 129359 },
                { LoadoutSlotType.GearMedicalSystem, 0 },
                { LoadoutSlotType.GearHead, 0 },
                { LoadoutSlotType.GearArms, 127501 },
                { LoadoutSlotType.GearLegs, 128271 },
                { LoadoutSlotType.GearReactor, 126731 },
                { LoadoutSlotType.GearOS, 129067 },
                { LoadoutSlotType.GearGadget1, 0 },
                { LoadoutSlotType.GearGadget2, 0 },
            }
        },
        

        // Advanced
        new LoadoutReferenceData
        {
            ChassisId = 76133, // Firecat
            SlottedItemsPvE = new Dictionary<LoadoutSlotType, uint>()
            {
                { LoadoutSlotType.Primary, 0 },
                { LoadoutSlotType.Secondary, 0 },
                { LoadoutSlotType.AbilityHKM, 0 },
                { LoadoutSlotType.Ability1, 0 },
                { LoadoutSlotType.Ability2, 0 },
                { LoadoutSlotType.Ability3, 0 },
                { LoadoutSlotType.Backpack, 78448 },
                { LoadoutSlotType.GearTorso, 0 },
                { LoadoutSlotType.GearAuxWeapon, 0 },
                { LoadoutSlotType.GearMedicalSystem, 0 },
                { LoadoutSlotType.GearHead, 0 },
                { LoadoutSlotType.GearArms, 0 },
                { LoadoutSlotType.GearLegs, 0 },
                { LoadoutSlotType.GearReactor, 0 },
                { LoadoutSlotType.GearOS, 0 },
                { LoadoutSlotType.GearGadget1, 0 },
                { LoadoutSlotType.GearGadget2, 0 },
            }
        },
        new LoadoutReferenceData
        {
            ChassisId = 76132, // Tigerclaw
            SlottedItemsPvE = new Dictionary<LoadoutSlotType, uint>()
            {
                { LoadoutSlotType.Primary, 0 },
                { LoadoutSlotType.Secondary, 0 },
                { LoadoutSlotType.AbilityHKM, 0 },
                { LoadoutSlotType.Ability1, 0 },
                { LoadoutSlotType.Ability2, 0 },
                { LoadoutSlotType.Ability3, 0 },
                { LoadoutSlotType.Backpack, 76022 },
                { LoadoutSlotType.GearTorso, 0 },
                { LoadoutSlotType.GearAuxWeapon, 0 },
                { LoadoutSlotType.GearMedicalSystem, 0 },
                { LoadoutSlotType.GearHead, 0 },
                { LoadoutSlotType.GearArms, 0 },
                { LoadoutSlotType.GearLegs, 0 },
                { LoadoutSlotType.GearReactor, 0 },
                { LoadoutSlotType.GearOS, 0 },
                { LoadoutSlotType.GearGadget1, 0 },
                { LoadoutSlotType.GearGadget2, 0 },
            }
        },
        new LoadoutReferenceData
        {
            ChassisId = 76337, // Electron
            SlottedItemsPvE = new Dictionary<LoadoutSlotType, uint>()
            {
                { LoadoutSlotType.Primary, 0 },
                { LoadoutSlotType.Secondary, 0 },
                { LoadoutSlotType.AbilityHKM, 0 },
                { LoadoutSlotType.Ability1, 0 },
                { LoadoutSlotType.Ability2, 0 },
                { LoadoutSlotType.Ability3, 0 },
                { LoadoutSlotType.Backpack, 75875 },
                { LoadoutSlotType.GearTorso, 0 },
                { LoadoutSlotType.GearAuxWeapon, 0 },
                { LoadoutSlotType.GearMedicalSystem, 0 },
                { LoadoutSlotType.GearHead, 0 },
                { LoadoutSlotType.GearArms, 0 },
                { LoadoutSlotType.GearLegs, 0 },
                { LoadoutSlotType.GearReactor, 0 },
                { LoadoutSlotType.GearOS, 0 },
                { LoadoutSlotType.GearGadget1, 0 },
                { LoadoutSlotType.GearGadget2, 0 },
            }
        },
        new LoadoutReferenceData
        {
            ChassisId = 76338, // Bastion
            SlottedItemsPvE = new Dictionary<LoadoutSlotType, uint>()
            {
                { LoadoutSlotType.Primary, 0 },
                { LoadoutSlotType.Secondary, 0 },
                { LoadoutSlotType.AbilityHKM, 0 },
                { LoadoutSlotType.Ability1, 0 },
                { LoadoutSlotType.Ability2, 0 },
                { LoadoutSlotType.Ability3, 0 },
                { LoadoutSlotType.Backpack, 76020 },
                { LoadoutSlotType.GearTorso, 0 },
                { LoadoutSlotType.GearAuxWeapon, 0 },
                { LoadoutSlotType.GearMedicalSystem, 0 },
                { LoadoutSlotType.GearHead, 0 },
                { LoadoutSlotType.GearArms, 0 },
                { LoadoutSlotType.GearLegs, 0 },
                { LoadoutSlotType.GearReactor, 0 },
                { LoadoutSlotType.GearOS, 0 },
                { LoadoutSlotType.GearGadget1, 0 },
                { LoadoutSlotType.GearGadget2, 0 },
            }
        },
        new LoadoutReferenceData
        {
            ChassisId = 76331, // Mammoth
            SlottedItemsPvE = new Dictionary<LoadoutSlotType, uint>()
            {
                { LoadoutSlotType.Primary, 134616 },
                { LoadoutSlotType.Secondary, 114316 },
                { LoadoutSlotType.AbilityHKM, 113931 },
                { LoadoutSlotType.Ability1, 143330 },
                { LoadoutSlotType.Ability2, 136056 },
                { LoadoutSlotType.Ability3, 113552 },
                { LoadoutSlotType.Backpack, 78041 },
                { LoadoutSlotType.GearTorso, 126575 },
                { LoadoutSlotType.GearAuxWeapon, 129458 },
                { LoadoutSlotType.GearMedicalSystem, 129056 },
                { LoadoutSlotType.GearHead, 125845 },
                { LoadoutSlotType.GearArms, 128036 },
                { LoadoutSlotType.GearLegs, 128766 },
                { LoadoutSlotType.GearReactor, 127306 },
                { LoadoutSlotType.GearOS, 129202 },
                { LoadoutSlotType.GearGadget1, 142078 },
                { LoadoutSlotType.GearGadget2, 130419 },
            }
        },
        new LoadoutReferenceData
        {
            ChassisId = 76332, // Rhino
            SlottedItemsPvE = new Dictionary<LoadoutSlotType, uint>()
            {
                { LoadoutSlotType.Primary, 0 },
                { LoadoutSlotType.Secondary, 0 },
                { LoadoutSlotType.AbilityHKM, 0 },
                { LoadoutSlotType.Ability1, 0 },
                { LoadoutSlotType.Ability2, 0 },
                { LoadoutSlotType.Ability3, 0 },
                { LoadoutSlotType.Backpack, 76018 },
                { LoadoutSlotType.GearTorso, 0 },
                { LoadoutSlotType.GearAuxWeapon, 0 },
                { LoadoutSlotType.GearMedicalSystem, 0 },
                { LoadoutSlotType.GearHead, 0 },
                { LoadoutSlotType.GearArms, 0 },
                { LoadoutSlotType.GearLegs, 0 },
                { LoadoutSlotType.GearReactor, 0 },
                { LoadoutSlotType.GearOS, 0 },
                { LoadoutSlotType.GearGadget1, 0 },
                { LoadoutSlotType.GearGadget2, 0 },
            }
        },
        new LoadoutReferenceData
        {
            ChassisId = 76335, // Dragonfly
            SlottedItemsPvE = new Dictionary<LoadoutSlotType, uint>()
            {
                { LoadoutSlotType.Primary, 0 },
                { LoadoutSlotType.Secondary, 0 },
                { LoadoutSlotType.AbilityHKM, 0 },
                { LoadoutSlotType.Ability1, 0 },
                { LoadoutSlotType.Ability2, 0 },
                { LoadoutSlotType.Ability3, 0 },
                { LoadoutSlotType.Backpack, 78449 },
                { LoadoutSlotType.GearTorso, 0 },
                { LoadoutSlotType.GearAuxWeapon, 0 },
                { LoadoutSlotType.GearMedicalSystem, 0 },
                { LoadoutSlotType.GearHead, 0 },
                { LoadoutSlotType.GearArms, 0 },
                { LoadoutSlotType.GearLegs, 0 },
                { LoadoutSlotType.GearReactor, 0 },
                { LoadoutSlotType.GearOS, 0 },
                { LoadoutSlotType.GearGadget1, 0 },
                { LoadoutSlotType.GearGadget2, 0 },
            }
        },
        new LoadoutReferenceData
        {
            ChassisId = 76336, // Recluse
            SlottedItemsPvE = new Dictionary<LoadoutSlotType, uint>()
            {
                { LoadoutSlotType.Primary, 0 },
                { LoadoutSlotType.Secondary, 0 },
                { LoadoutSlotType.AbilityHKM, 0 },
                { LoadoutSlotType.Ability1, 0 },
                { LoadoutSlotType.Ability2, 0 },
                { LoadoutSlotType.Ability3, 0 },
                { LoadoutSlotType.Backpack, 76019 },
                { LoadoutSlotType.GearTorso, 0 },
                { LoadoutSlotType.GearAuxWeapon, 0 },
                { LoadoutSlotType.GearMedicalSystem, 0 },
                { LoadoutSlotType.GearHead, 0 },
                { LoadoutSlotType.GearArms, 0 },
                { LoadoutSlotType.GearLegs, 0 },
                { LoadoutSlotType.GearReactor, 0 },
                { LoadoutSlotType.GearOS, 0 },
                { LoadoutSlotType.GearGadget1, 0 },
                { LoadoutSlotType.GearGadget2, 0 },
            }
        },
        new LoadoutReferenceData
        {
            ChassisId = 76333, // Nighthawk
            SlottedItemsPvE = new Dictionary<LoadoutSlotType, uint>()
            {
                { LoadoutSlotType.Primary, 0 },
                { LoadoutSlotType.Secondary, 0 },
                { LoadoutSlotType.AbilityHKM, 0 },
                { LoadoutSlotType.Ability1, 0 },
                { LoadoutSlotType.Ability2, 0 },
                { LoadoutSlotType.Ability3, 0 },
                { LoadoutSlotType.Backpack, 76021 },
                { LoadoutSlotType.GearTorso, 0 },
                { LoadoutSlotType.GearAuxWeapon, 0 },
                { LoadoutSlotType.GearMedicalSystem, 0 },
                { LoadoutSlotType.GearHead, 0 },
                { LoadoutSlotType.GearArms, 0 },
                { LoadoutSlotType.GearLegs, 0 },
                { LoadoutSlotType.GearReactor, 0 },
                { LoadoutSlotType.GearOS, 0 },
                { LoadoutSlotType.GearGadget1, 0 },
                { LoadoutSlotType.GearGadget2, 0 },
            }
        },
        new LoadoutReferenceData
        {
            ChassisId = 76334, // Raptor
            SlottedItemsPvE = new Dictionary<LoadoutSlotType, uint>()
            {
                { LoadoutSlotType.Primary, 0 },
                { LoadoutSlotType.Secondary, 0 },
                { LoadoutSlotType.AbilityHKM, 0 },
                { LoadoutSlotType.Ability1, 0 },
                { LoadoutSlotType.Ability2, 0 },
                { LoadoutSlotType.Ability3, 0 },
                { LoadoutSlotType.Backpack, 78040 },
                { LoadoutSlotType.GearTorso, 0 },
                { LoadoutSlotType.GearAuxWeapon, 0 },
                { LoadoutSlotType.GearMedicalSystem, 0 },
                { LoadoutSlotType.GearHead, 0 },
                { LoadoutSlotType.GearArms, 0 },
                { LoadoutSlotType.GearLegs, 0 },
                { LoadoutSlotType.GearReactor, 0 },
                { LoadoutSlotType.GearOS, 0 },
                { LoadoutSlotType.GearGadget1, 0 },
                { LoadoutSlotType.GearGadget2, 0 },
            }
        },
        */
    ];

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

    public static void GenerateCharCreateLoadoutAndItems(CharacterInventory inventory, uint charCreateLoadoutId, uint chassisId)
    {
        LoadoutReferenceData refData = new()
        {
            ChassisId = chassisId,
        };

        Dictionary<byte, CharCreateLoadoutSlots> defaultSlots = SDBUtils.GetDefaultLoadoutSlots(charCreateLoadoutId);

        if (defaultSlots != null)
        {
            foreach (LoadoutSlotType slot in defaultSlots.Keys)
            {
                if (CharacterLoadout.LoadoutAbilitySlots.Contains(slot) || CharacterLoadout.LoadoutChassisSlots.Contains(slot) || CharacterLoadout.LoadoutWeaponSlots.Contains(slot))
                {
                    CharCreateLoadoutSlots record = defaultSlots.GetValueOrDefault((byte)slot);
                    if (record.DefaultPveModule != 0)
                    {
                        Console.WriteLine($"XA Chassis {chassisId} {slot} {record.DefaultPveModule}");
                        refData.SlottedItemsPvE.Add(slot, record.DefaultPveModule);
                    }

                    if (record.DefaultPvpModule != 0)
                    {
                        refData.SlottedItemsPvP.Add(slot, record.DefaultPvpModule);
                    }
                }
            }
        }

        GenerateLoadoutAndItems(inventory, refData);
    }

    public static void GenerateLoadoutAndItems(CharacterInventory inventory, LoadoutReferenceData sourceData)
    {
        var loadoutId = sourceData.LoadoutId == 0 ? GeneratedLoadoutCounter++ : sourceData.LoadoutId;
        var loadout = new Loadout()
        {
            FrameLoadoutId = loadoutId,
            ChassisID = sourceData.ChassisId,
            LoadoutName = $"Loadout {loadoutId}",
            LoadoutType = "battleframe",
        };

        var chassisGuid = inventory.CreateItem(sourceData.ChassisId);

        var pveConfig = new LoadoutConfig()
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
        };

        var pveItems = new List<LoadoutConfig_Item>();
        foreach (var (slot, typeId) in sourceData.SlottedItemsPvE)
        {
            if (typeId == 0)
            {
                continue;
            }

            var guid = inventory.CreateItem(typeId);
            pveItems.Add(new LoadoutConfig_Item() { ItemGUID = guid, SlotIndex = (byte)slot });
        }

        pveConfig.Items = pveItems.ToArray();

        var pvpConfig = new LoadoutConfig()
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
        };

        var pvpItems = new List<LoadoutConfig_Item>();
        foreach (var (slot, typeId) in sourceData.SlottedItemsPvP)
        {
            if (typeId == 0)
            {
                continue;
            }

            var guid = inventory.CreateItem(typeId);
            pvpItems.Add(new LoadoutConfig_Item() { ItemGUID = guid, SlotIndex = (byte)slot });
        }

        pvpConfig.Items = pvpItems.ToArray();

        loadout.LoadoutConfigs = 
        [
            pveConfig,
            pvpConfig
        ];

        inventory.AddLoadout(loadout);
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

public class LoadoutReferenceData
{
    public uint LoadoutId;
    public uint ChassisId;
    public Dictionary<LoadoutSlotType, uint> SlottedItemsPvE = new Dictionary<LoadoutSlotType, uint>();
    public Dictionary<LoadoutSlotType, uint> SlottedItemsPvP = new Dictionary<LoadoutSlotType, uint>();
}