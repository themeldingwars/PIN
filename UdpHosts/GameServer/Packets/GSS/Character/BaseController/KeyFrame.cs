using GameServer.Enums.GSS.Character;
using Shared.Udp;
using System;
using System.Collections.Generic;

namespace GameServer.Packets.GSS.Character.BaseController
{
    [GSSMessage(Enums.GSS.Controllers.Character_BaseController, (byte)Events.KeyFrame)]
    public class KeyFrame
    {
        [Field] [LengthPrefixed(typeof(byte))] public IList<Ability> Abilities = new List<Ability>();

        [Field] public float AimX;

        [Field] public float AimY;

        [Field] public float AimZ;

        [Field] public ushort AirGroundTimer;

        [Field] [Padding(9)] public ulong ArmyID;

        [Field]
        //[Padding(5)]
        public string ArmyName;

        [Field] [Padding(4)] [LengthPrefixed(typeof(ushort))]
        public IList<StatValue> AttribCats1 = new List<StatValue>();

        [Field] [LengthPrefixed(typeof(ushort))]
        public IList<StatValue> AttribCats2 = new List<StatValue>();

        [Field] public uint BackpackLoadout;

        [Field] [Padding(1)] public byte CharacterState;

        [Field] public Visuals CharacterVisuals = new();

        [Field] public uint CharInfoID;

        [Field] public uint CharTypeID;

        [Field] [Padding(1)] public uint ChassisLoadout;

        [Field] public Visuals ChassisVisuals = new();

        [Field] public uint CurrentHealth;

        [Field] [Padding(17)] public string DisplayName;

        [Field] public byte EffectsFlag;

        [Field] public uint Eyes;

        [Field] public byte FactionID;

        [Field] public byte FactionMode;

        [Field] [LengthPrefixed(typeof(byte))] public IList<GearItem> Gear = new List<GearItem>();

        [Field] public byte Gender;

        [Field] public uint GliderLoadout;

        [Field] [LengthPrefixed(typeof(byte))] public IList<uint> HeadAccessories = new List<uint>();

        [Field] public uint HeadMain;

        [Field] public ulong InstanceID;

        [Field] public byte IsNPC;

        [Field] public byte IsStaff;

        [Field] [LengthPrefixed(typeof(ushort))]
        public IList<StatValue> ItemStatValues = new List<StatValue>();

        [Field] [Padding(2)] public ushort Jets;

        [Field] public float JumpJetEnergy;

        [Field] public float JumpJetRecharge;

        [Field] public ushort JumpTimer;

        [Field] public uint KeyFrameTime_0;

        [Field] public uint KeyFrameTime_1;

        [Field] public uint KeyFrameTime_2;

        [Field] [Padding(8)] public uint KeyFrameTime_3;

        [Field] public uint KeyFrameTime_4;

        [Field] public uint KeyFrameTime_5;

        [Field] [Padding(13)] public uint LoadoutID;

        [Field] public uint MaxHealth;

        [Field] public uint MaxInvSlots;

        [Field] public uint MaxJumpJetEnergy;

        [Field] public ushort MovementState;

        [Field] public uint NameLocaliztionID;

        [Field]
        //[Padding(4)]
        public float PosX;

        [Field] public float PosY;

        [Field] public float PosZ;

        [Field] [Padding(9)] public uint PrimaryWeaponID;

        [Field] [Padding(3)] [LengthPrefixed(typeof(byte))]
        public IList<WeaponModule> PrimaryWeaponModules = new List<WeaponModule>();

        [Field] public Visuals PrimaryWeaponVisuals = new();

        [Field] public byte Race;

        [Field] public float RotW;

        [Field] public float RotX;

        [Field] public float RotY;

        [Field] public float RotZ;

        [Field] [Padding(8)] public uint SecondaryWeaponID;

        [Field] [LengthPrefixed(typeof(byte))] public IList<WeaponModule> SecondaryWeaponModules = new List<WeaponModule>();

        [Field] public Visuals SecondaryWeaponVisuals = new();

        [Field] public ushort TitleID;

        [Field] public string UniqueName;

        [Field] [Length(292)] // This matches Xsear's packet
        //[Length(204)] // Minimum allowed if all zeros?
        public IList<byte> Unk = new List<byte>();

        [Field] public byte UnkByte1;

        [Field] [Length(3)] public IList<byte> UnkBytes = new List<byte>();

        [Field] [Length(3)] public IList<byte> UnkBytes2 = new List<byte>();

        [Field] [Length(3)] public IList<byte> UnkBytes5 = new List<byte>();

        [Field] public uint UnkInt1;

        [Field] public uint UnkInt2;

        [Field] public uint UnkSfxID_0a;

        [Field] public ulong UnkSfxID_0b;

        [Field] [Padding(1)] public uint UnkSfxID_0d;

        [Padding(1)] [Field] public uint UsedInvSlots;

        [Field] public uint VehicleLoadout;

        [Field] public float VelX;

        [Field] public float VelY;

        [Field] public float VelZ;

        [Field] public uint VoiceSet;

        [Field] [Padding(4)] [LengthPrefixed(typeof(ushort))]
        public IList<StatValue> Weapon1StatValues = new List<StatValue>();

        [Field] [Padding(4)] [LengthPrefixed(typeof(ushort))]
        public IList<StatValue> Weapon2StatValues = new List<StatValue>();

        public class Palette
        {
            [Field] public uint ID;

            [Field] public byte Type;
        }

        public class StatValue
        {
            [Field] public ushort StatID;

            [Field] public float Value;
        }

        public class Pattern
        {
            [Field] public uint ID;

            [Field] [Length(4)] public IList<Half> Transform = new List<Half>();

            [Field] public byte Usage;
        }

        public class Visuals
        {
            [Field] [LengthPrefixed(typeof(byte))] public IList<uint> Colors = new List<uint>();

            [Field] [LengthPrefixed(typeof(byte))] public IList<DecalGradient> DecalGradients = new List<DecalGradient>();

            [Field] [LengthPrefixed(typeof(byte))] public IList<Decal> Decals = new List<Decal>();

            // Below are likely Morph Weights, Overlays and Pattern Gradients?

            [Field] [LengthPrefixed(typeof(byte))] public IList<uint> ItemsUnk1 = new List<uint>();

            [Field] [LengthPrefixed(typeof(byte))] public IList<uint> ItemsUnk2 = new List<uint>();

            [Field] [LengthPrefixed(typeof(byte))] public IList<uint> ItemsUnk3 = new List<uint>();

            [Field] [LengthPrefixed(typeof(byte))] public IList<uint> OrnamentGroups = new List<uint>();

            [Field] [LengthPrefixed(typeof(byte))] public IList<Palette> Palettes = new List<Palette>();

            [Field] [LengthPrefixed(typeof(byte))] public IList<Pattern> Patterns = new List<Pattern>();
        }

        public class GearItem
        {
            [Field] public uint ID;

            [Field] [Length(3)] public IList<byte> Unk = new List<byte>();
        }

        public class Ability
        {
            [Field] [LengthPrefixed(typeof(byte))] public IList<AbilityModule> AbilityModules = new List<AbilityModule>();

            [Field] public uint ID;

            [Field] public byte Slot;

            [Field] public byte UnkByte1;
        }

        public class AbilityModule
        {
            [Field] public uint ID;

            [Field] public uint Unk;
        }

        public class Decal
        {
            [Field] public uint Color;

            [Field] public uint ID;

            [Field] [Length(12)] public IList<Half> Transform = new List<Half>();

            [Field] public byte Usage;
        }

        public class DecalGradient
        {
            [Field] public byte Unk;
        }

        public class WeaponModule
        {
            [Field] public uint ID;

            [Field] [Length(3)] public IList<byte> Unk = new List<byte>();
        }
    }
}