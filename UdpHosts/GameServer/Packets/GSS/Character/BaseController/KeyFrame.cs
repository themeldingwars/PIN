using GameServer.Enums.GSS.Character;
using Shared.Udp;
using System;
using System.Collections.Generic;

namespace GameServer.Packets.GSS.Character.BaseController;

[GSSMessage(Enums.GSS.Controllers.Character_BaseController, (byte)Events.KeyFrame)]
public class KeyFrame
{
    [Field]
    public ulong InstanceID;

    [Field]
    public uint UnkInt1;

    [Field]
    public uint UnkInt2;

    [Padding(1)]
    [Field]
    public uint UsedInvSlots;

    [Field]
    public uint MaxInvSlots;

    [Field]
    [Padding(17)]
    public string DisplayName;

    [Field]
    public string UniqueName;

    [Field]
    public byte Gender;

    [Field]
    public byte Race;

    [Field]
    public uint CharInfoID;

    [Field]
    public uint HeadMain;

    [Field]
    public uint Eyes;

    [Field]
    public byte UnkByte1;

    [Field]
    public byte IsNPC;

    [Field]
    public byte IsStaff;

    [Field]
    public uint CharTypeID;

    [Field]
    public uint VoiceSet;

    [Field]
    public ushort TitleID;

    [Field]
    public uint NameLocalizationID;

    [Field]
    [LengthPrefixed(typeof(byte))]
    public IList<uint> HeadAccessories = new List<uint>();

    [Field]
    public uint VehicleLoadout;

    [Field]
    public uint GliderLoadout;

    [Field]
    public Visuals CharacterVisuals = new();

    [Field]
    //[Padding(5)]
    public string ArmyName;

    [Field]
    public uint KeyFrameTime_0;

    [Field]
    [Padding(1)]
    public uint ChassisLoadout;

    [Field]
    [Length(3)]
    public IList<byte> UnkBytes = new List<byte>();

    [Field]
    [LengthPrefixed(typeof(byte))]
    public IList<GearItem> Gear = new List<GearItem>();

    [Field]
    public Visuals ChassisVisuals = new();

    [Field]
    public uint BackpackLoadout;

    [Field]
    [Length(3)]
    public IList<byte> UnkBytes2 = new List<byte>();

    [Field]
    [LengthPrefixed(typeof(byte))]
    public IList<Ability> Abilities = new List<Ability>();

    [Field]
    [Padding(9)]
    public uint PrimaryWeaponID;

    [Field]
    [Padding(3)]
    [LengthPrefixed(typeof(byte))]
    public IList<WeaponModule> PrimaryWeaponModules = new List<WeaponModule>();

    [Field]
    public Visuals PrimaryWeaponVisuals = new();

    [Field]
    [Padding(8)]
    public uint SecondaryWeaponID;

    [Field]
    [Length(3)]
    public IList<byte> UnkBytes5 = new List<byte>();

    [Field]
    [LengthPrefixed(typeof(byte))]
    public IList<WeaponModule> SecondaryWeaponModules = new List<WeaponModule>();

    [Field]
    public Visuals SecondaryWeaponVisuals = new();

    [Field]
    [Padding(13)]
    public uint LoadoutID;

    [Field]
    public uint UnkSfxID_0a;

    [Field]
    public ulong UnkSfxID_0b;

    [Field]
    public uint KeyFrameTime_1;

    [Field]
    //[Padding(4)]
    public float PosX;

    [Field]
    public float PosY;

    [Field]
    public float PosZ;

    [Field]
    public float RotW;

    [Field]
    public float RotX;

    [Field]
    public float RotY;

    [Field]
    public float RotZ;

    [Field]
    public float AimX;

    [Field]
    public float AimY;

    [Field]
    public float AimZ;

    [Field]
    public float VelX;

    [Field]
    public float VelY;

    [Field]
    public float VelZ;

    [Field]
    public ushort MovementState;

    [Field]
    [Padding(2)]
    public ushort Jets;

    [Field]
    public ushort AirGroundTimer;

    [Field]
    public ushort JumpTimer;

    [Field]
    [Padding(1)]
    public uint UnkSfxID_0d;

    [Field]
    [Padding(1)]
    public byte CharacterState;

    [Field]
    public uint KeyFrameTime_2;

    [Field]
    public byte FactionMode;

    [Field]
    public byte FactionID;

    [Field]
    public uint CurrentHealth;

    [Field]
    [Padding(8)]
    public uint KeyFrameTime_3;

    [Field]
    public uint MaxHealth;

    [Field]
    public uint KeyFrameTime_4;

    [Field]
    public byte EffectsFlag;

    [Field]
    public float JumpJetEnergy;

    [Field]
    public uint MaxJumpJetEnergy;

    [Field]
    public float JumpJetRecharge;

    [Field]
    public uint KeyFrameTime_5;

    [Field]
    [LengthPrefixed(typeof(ushort))]
    public IList<StatValue> ItemStatValues = new List<StatValue>();

    [Field]
    [Padding(4)]
    [LengthPrefixed(typeof(ushort))]
    public IList<StatValue> Weapon1StatValues = new List<StatValue>();

    [Field]
    [Padding(4)]
    [LengthPrefixed(typeof(ushort))]
    public IList<StatValue> Weapon2StatValues = new List<StatValue>();

    [Field]
    [Padding(4)]
    [LengthPrefixed(typeof(ushort))]
    public IList<StatValue> AttribCats1 = new List<StatValue>();

    [Field]
    [LengthPrefixed(typeof(ushort))]
    public IList<StatValue> AttribCats2 = new List<StatValue>();

    [Field]
    [Padding(9)]
    public ulong ArmyID;

    [Field]
    [Length(292)] // This matches Xsear's packet
    //[Length(204)] // Minimum allowed if all zeros?
    public IList<byte> Unk = new List<byte>();

    public class Palette
    {
        [Field]
        public byte Type;

        [Field]
        public uint ID;
    }

    public class StatValue
    {
        [Field]
        public ushort StatID;

        [Field]
        public float Value;
    }

    public class Pattern
    {
        [Field]
        public uint ID;

        [Field]
        [Length(4)]
        public IList<Half> Transform = new List<Half>();

        [Field]
        public byte Usage;
    }

    public class Visuals
    {
        [Field]
        [LengthPrefixed(typeof(byte))]
        public IList<Decal> Decals = new List<Decal>();

        [Field]
        [LengthPrefixed(typeof(byte))]
        public IList<DecalGradient> DecalGradients = new List<DecalGradient>();

        [Field]
        [LengthPrefixed(typeof(byte))]
        public IList<uint> Colors = new List<uint>();

        [Field]
        [LengthPrefixed(typeof(byte))]
        public IList<Palette> Palettes = new List<Palette>();

        [Field]
        [LengthPrefixed(typeof(byte))]
        public IList<Pattern> Patterns = new List<Pattern>();

        [Field]
        [LengthPrefixed(typeof(byte))]
        public IList<uint> OrnamentGroups = new List<uint>();

        // Below are likely Morph Weights, Overlays and Pattern Gradients?

        [Field]
        [LengthPrefixed(typeof(byte))]
        public IList<uint> ItemsUnknown1 = new List<uint>();

        [Field]
        [LengthPrefixed(typeof(byte))]
        public IList<uint> ItemsUnknown2 = new List<uint>();

        [Field]
        [LengthPrefixed(typeof(byte))]
        public IList<uint> ItemsUnknown3 = new List<uint>();
    }

    public class GearItem
    {
        [Field]
        public uint ID;

        [Field]
        [Length(3)]
        public IList<byte> Unk = new List<byte>();
    }

    public class Ability
    {
        [Field]
        public uint ID;

        [Field]
        public byte Slot;

        [Field]
        [LengthPrefixed(typeof(byte))]
        public IList<AbilityModule> AbilityModules = new List<AbilityModule>();

        [Field]
        public byte UnkByte1;
    }

    public class AbilityModule
    {
        [Field]
        public uint Unk;

        [Field]
        public uint ID;
    }

    public class Decal
    {
        [Field]
        public uint ID;

        [Field]
        public uint Color;

        [Field]
        [Length(12)]
        public IList<Half> Transform = new List<Half>();

        [Field]
        public byte Usage;
    }

    public class DecalGradient
    {
        [Field]
        public byte Unk;
    }

    public class WeaponModule
    {
        [Field]
        public uint ID;

        [Field]
        [Length(3)]
        public IList<byte> Unk = new List<byte>();
    }
}