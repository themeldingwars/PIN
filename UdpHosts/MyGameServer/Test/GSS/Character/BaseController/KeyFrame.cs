using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using MyGameServer.Extensions;

using Packet = MyGameServer.Packets.GSS.Character.BaseController;

namespace MyGameServer.Test.GSS.Character.BaseController {
	internal static class KeyFrame {
		public static Packet.KeyFrame Test( IPlayer p, IInstance inst ) {
			var gametime = 0xAAAAAAAAu;
			var ret = new Packet.KeyFrame();
			var cd = p.CharacterEntity.CharData;

			ret.KeyFrameTime_0 = gametime;
			ret.KeyFrameTime_1 = gametime;
			ret.KeyFrameTime_2 = gametime;
			ret.KeyFrameTime_3 = gametime;
			ret.KeyFrameTime_4 = gametime;
			ret.KeyFrameTime_5 = gametime;

			ret.InstanceID = inst.InstanceID;
			ret.UnkInt1 = 0xffffffffu;
			ret.UnkInt2 = 0x0000003fu;

			ret.UsedInvSlots = 0;
			ret.MaxInvSlots = cd.MaxInventorySlots;

			ret.DisplayName = cd.Name;
			ret.UniqueName = cd.Name;
			ret.Gender = (byte)cd.Gender;
			ret.Race = (byte)cd.Race;
			ret.CharInfoID = cd.CharInfoID;
			ret.HeadMain = cd.CharVisuals.HeadMain;
			ret.Eyes = cd.CharVisuals.Eyes;
			ret.UnkByte1 = 0xff;
			ret.IsNPC = 0;
			ret.IsStaff = 0x3;

			ret.CharTypeID = cd.CharVisuals.CharTypeID;
			ret.VoiceSet = cd.VoiceSet;
			ret.TitleID = cd.TitleID;
			ret.NameLocaliztionID = cd.NameLocalizationID;

			ret.HeadAccessories = cd.CharVisuals.HeadAccessories;

			ret.VehicleLoadout = cd.Loadout.VehicleID;
			ret.GliderLoadout = cd.Loadout.GliderID;

			ret.CharacterVisuals = cd.CharVisuals;

			ret.ArmyName = cd.Army.Name;

			ret.ChassisLoadout = cd.Loadout.ChassisID;

			ret.UnkBytes = new byte[] { 0xff, 0x00, 0x00 };

			ret.Gear.AddAll( cd.Loadout.ChassisModules.Select( x => new Packet.KeyFrame.GearItem { ID = x, Unk = new byte[] { 0xff, 0x00, 0x00 } } ) );

			ret.ChassisVisuals = cd.ChassisVisuals;
			ret.BackpackLoadout = cd.Loadout.BackpackID;
			ret.UnkBytes2 = new byte[] { 0xff, 0x00, 0x00 };

			ret.Abilities.AddAll( cd.Loadout.BackpackModules.Select( x => (Packet.KeyFrame.Ability)x ) );

			ret.PrimaryWeaponID = cd.Loadout.PrimaryWeaponID;
			ret.PrimaryWeaponModules.AddAll( cd.Loadout.PrimaryWeaponModules.Select( x => (Packet.KeyFrame.WeaponModule)x ) );
			ret.PrimaryWeaponVisuals = cd.Loadout.PrimaryWeaponVisuals;

			ret.SecondaryWeaponID = cd.Loadout.SecondaryWeaponID;
			ret.UnkBytes5 = new byte[] { 0x01, 0x00, 0x00 }.ToList();
			ret.SecondaryWeaponModules.AddAll( cd.Loadout.SecondaryWeaponModules.Select( x => (Packet.KeyFrame.WeaponModule)x ) );
			ret.SecondaryWeaponVisuals = cd.Loadout.SecondaryWeaponVisuals;

			ret.LoadoutID = cd.Loadout.GUID;

			ret.PosX = p.CharacterEntity.Position.X;
			ret.PosY = p.CharacterEntity.Position.Y;
			ret.PosZ = p.CharacterEntity.Position.Z;

			ret.RotW = 0;
			ret.RotX = 0;
			ret.RotY = 0.394583433866501f; // 0xd7, 0x06, 0xca, 0x3e 0x3eca06d7
			ret.RotZ = 0.918860137462616f; // 0x6b, 0x3a, 0x6b, 0x3f 0x3f6b3a6b

			ret.AimX = 0.725133955478668f; // 0x61, 0xa2, 0x39, 0x3f 0x3f39a261
			ret.AimY = 0.688607811927795f; // 0x9a, 0x48, 0x30, 0x3f 0x3f30489a
			ret.AimZ = 0; // 0

			ret.VelX = 0;
			ret.VelY = 0;
			ret.VelZ = 0;

			ret.MovementState = 0x1000;
			ret.Jets = 0x639c;
			ret.AirGroundTimer = 0;
			ret.JumpTimer = 0;

			ret.UnkSfxID_0d = 0x012c0a37;

			ret.CharacterState = 0;

			ret.FactionMode = cd.Faction.Mode;
			ret.FactionID = cd.Faction.ID;

			ret.CurrentHealth = 25000;
			ret.MaxHealth = 25000;
			ret.EffectsFlag = 0x52;
			ret.JumpJetEnergy = cd.JumpJetEnergy;
			ret.MaxJumpJetEnergy = cd.MaxJumpJetEnergy;
			ret.JumpJetRecharge = cd.JumpJetRecharge;

			ret.ItemStatValues.AddAll( new[] {
				new Packet.KeyFrame.StatValue{ StatID = 5, Value = 156.414169f },
				new Packet.KeyFrame.StatValue{ StatID = 6, Value = 1037.8347f },
				new Packet.KeyFrame.StatValue{ StatID = 7, Value = 177.44128f },
				new Packet.KeyFrame.StatValue{ StatID = 12, Value = 16.250000f },
				new Packet.KeyFrame.StatValue{ StatID = 35, Value = 300 },
				new Packet.KeyFrame.StatValue{ StatID = 36, Value = 250 },
				new Packet.KeyFrame.StatValue{ StatID = 37, Value = 2.092090f },
				new Packet.KeyFrame.StatValue{ StatID = 142, Value = 12.55f },
				new Packet.KeyFrame.StatValue{ StatID = 143, Value = 1136 },
				new Packet.KeyFrame.StatValue{ StatID = 144, Value = 18.433180f },
				new Packet.KeyFrame.StatValue{ StatID = 173, Value = 10 },
				new Packet.KeyFrame.StatValue{ StatID = 186, Value = 11.40f },
				new Packet.KeyFrame.StatValue{ StatID = 959, Value = 1 },
				new Packet.KeyFrame.StatValue{ StatID = 1050, Value = 34.5f },
				new Packet.KeyFrame.StatValue{ StatID = 1051, Value = 13.824884f },
				new Packet.KeyFrame.StatValue{ StatID = 1052, Value = 5.5f },
				new Packet.KeyFrame.StatValue{ StatID = 1121, Value = 150 },
				new Packet.KeyFrame.StatValue{ StatID = 1146, Value = 10.0f },
				new Packet.KeyFrame.StatValue{ StatID = 1367, Value = 85 },
				new Packet.KeyFrame.StatValue{ StatID = 1368, Value = 100 },
				new Packet.KeyFrame.StatValue{ StatID = 1370, Value = 65 },
				new Packet.KeyFrame.StatValue{ StatID = 1371, Value = 120 },
				new Packet.KeyFrame.StatValue{ StatID = 1372, Value = 140 },
				new Packet.KeyFrame.StatValue{ StatID = 1377, Value = 140.531250f },
				new Packet.KeyFrame.StatValue{ StatID = 1395, Value = 75 },
				new Packet.KeyFrame.StatValue{ StatID = 1419, Value = 32.769249f },
				new Packet.KeyFrame.StatValue{ StatID = 1420, Value = 16901.744141f },
				new Packet.KeyFrame.StatValue{ StatID = 1439, Value = 15279.667969f },
				new Packet.KeyFrame.StatValue{ StatID = 1451, Value = 681 },
				new Packet.KeyFrame.StatValue{ StatID = 1583, Value = 1 },
				new Packet.KeyFrame.StatValue{ StatID = 1620, Value = 5049.767090f },
				new Packet.KeyFrame.StatValue{ StatID = 1622, Value = 8 },
				new Packet.KeyFrame.StatValue{ StatID = 1733, Value = 1.800000f },
				new Packet.KeyFrame.StatValue{ StatID = 1736, Value = 60 },
				new Packet.KeyFrame.StatValue{ StatID = 1737, Value = 5486.919434f },
				new Packet.KeyFrame.StatValue{ StatID = 1746, Value = 9.320923f },
				new Packet.KeyFrame.StatValue{ StatID = 1785, Value = 1.084000f },
				new Packet.KeyFrame.StatValue{ StatID = 1835, Value = 5932.512207f },
				new Packet.KeyFrame.StatValue{ StatID = 1904, Value = 4 },
				new Packet.KeyFrame.StatValue{ StatID = 1905, Value = 2 },
				new Packet.KeyFrame.StatValue{ StatID = 1987, Value = 8 },
				new Packet.KeyFrame.StatValue{ StatID = 2034, Value = 22 },
				new Packet.KeyFrame.StatValue{ StatID = 2037, Value = 9887.518555f },
				new Packet.KeyFrame.StatValue{ StatID = 2039, Value = 9 },
				new Packet.KeyFrame.StatValue{ StatID = 2042, Value = 12.252850f },
			} );

			ret.Weapon1StatValues.AddAll( new[] {
				new Packet.KeyFrame.StatValue{ StatID = 17, Value = 125.144996643f },
				new Packet.KeyFrame.StatValue{ StatID = 20, Value = 30.0f },
				new Packet.KeyFrame.StatValue{ StatID = 23, Value = 0.625724971294f },
				new Packet.KeyFrame.StatValue{ StatID = 29, Value = 0.5f },
				new Packet.KeyFrame.StatValue{ StatID = 954, Value = 402.857513428f },
				new Packet.KeyFrame.StatValue{ StatID = 956, Value = 264.367950439f },
				new Packet.KeyFrame.StatValue{ StatID = 957, Value = 124.0f },
				new Packet.KeyFrame.StatValue{ StatID = 958, Value = 4.0f },
				new Packet.KeyFrame.StatValue{ StatID = 1543, Value = 14.0f },
				new Packet.KeyFrame.StatValue{ StatID = 1546, Value = 30.0f },
				new Packet.KeyFrame.StatValue{ StatID = 1666, Value = 100.0f },
				new Packet.KeyFrame.StatValue{ StatID = 1810, Value = 1000.0f },
				new Packet.KeyFrame.StatValue{ StatID = 1811, Value = 4.0f },
			} );

			ret.Weapon2StatValues.AddAll( new[] {
				new Packet.KeyFrame.StatValue{ StatID = 17, Value = 90.0f },
				new Packet.KeyFrame.StatValue{ StatID = 20, Value = 30.0f },
				new Packet.KeyFrame.StatValue{ StatID = 954, Value = 136.50453186f },
				new Packet.KeyFrame.StatValue{ StatID = 956, Value = 40.0f },
				new Packet.KeyFrame.StatValue{ StatID = 957, Value = 125.0f },
				new Packet.KeyFrame.StatValue{ StatID = 958, Value = 5.0f },
				new Packet.KeyFrame.StatValue{ StatID = 1666, Value = 100.0f },
				new Packet.KeyFrame.StatValue{ StatID = 1811, Value = 3.0f },
			} );

			ret.AttribCats1.AddAll( new[] {
				new Packet.KeyFrame.StatValue{ StatID = 10003, Value = 0.0f },
			} );

			ret.AttribCats2.AddAll( new[] {
				new Packet.KeyFrame.StatValue{ StatID = 10003, Value = 4.310000f },
				new Packet.KeyFrame.StatValue{ StatID = 10008, Value = 0.093200f },
				new Packet.KeyFrame.StatValue{ StatID = 10010, Value = 0.102000f },
				new Packet.KeyFrame.StatValue{ StatID = 10035, Value = 0.910400f },
				new Packet.KeyFrame.StatValue{ StatID = 10045, Value = 0.093200f },
				new Packet.KeyFrame.StatValue{ StatID = 10052, Value = 0.171000f },
			} );

			ret.ArmyID = cd.Army.GUID;

			// TODO: finish
			ret.Unk = new List<byte>() {
				0x01,
				0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00,

				0xfd, 0xdf, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00,

				0xff, 0xff, 0xff, 0x3f, 0xff, 0x7f, 0x00, 0x00,

				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

				0x00, 0x00, 0x00, 0x00,
				0x01, 0x00, 0x00, 0x00,
				0x00,

				0x00, 0x00, 0x00, 0x00,

				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f,
				0xdd, 0x02, 0x00, 0x00, 0x88, 0xed, 0x31, 0x57,

				0x00,
				0x00,

				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01,
				0x00, 0x00,
				0x2c, 0x00, 0x00, 0x00,

				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00,

				0x75, 0x01,
				0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00,
				0xcb, 0x10, 0x00, 0x00,
				0x10, 0x00, 0x00, 0x00
			};

			return ret;
		}
	}
}
