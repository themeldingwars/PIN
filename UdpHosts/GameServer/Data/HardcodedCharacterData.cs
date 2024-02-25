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