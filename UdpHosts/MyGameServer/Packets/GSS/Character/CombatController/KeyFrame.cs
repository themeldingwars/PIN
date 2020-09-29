using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.GSS.Character.CombatController {
	[GSSMessage( Enums.GSS.Controllers.Character_CombatController, (byte)Enums.GSS.Character.Events.KeyFrame )]
	public class KeyFrame {
		[Field]
		public ulong PlayerID;
		[Field]
		[Length(294)]
		public IList<byte> UnkBytes;

		public KeyFrame( IShard shard ) {
            var c = shard.CurrentTime;
            var b = Utils.WritePrimitive( c ).ToArray();

            UnkBytes = new List<byte> {
                0xff, 0xff, 0xff, 0xff, // Identical 4 bytes across all these
                0xff, 0x03,
                // not sure if we have a longer header or not

                0xa8, 0xff, // 0
                0x98, 0x4e, // 1
                0x15, 0x1b, // 2
                0x98, 0x4e, // 3
                0x00, 0x4a, // 4
                0x28, 0x18, // 5
                0x43, 0x52, // 6

                0x6c, 0x55, // 7
                0x32, 0x36, // 8
                0x30, 0x32, // 9
                0x37, 0x32, // a
                0x39, 0x30, // b
                0x00, 0x34, // c
                0x37, 0x32, // d
                0x01, 0x00,
                0x00, 0x00,

                0x01, 0x00,
                0x00, 0x00,

                b[3], b[2], b[1], b[0],
                0x00, 0x00, 0x80, 0x3f,
                b[3], b[2], b[1], b[0],
                b[3], b[2], b[1], b[0],

                0x01, 0x61, 0x6e, 0x74,
                0x00, 0x00, 0x00, 0x00,
                0x01, 0x00, 0x00, 0x00,

                // 40
                0x00, 0x00, 0x80, 0x3f,
                b[3], b[2], b[1], b[0],

                // 41
                0x00, 0x00, 0x80, 0x3f,
                b[3], b[2], b[1], b[0],

                // 42
                0x00, 0x00, 0x80, 0x3f,
                b[3], b[2], b[1], b[0],

                // 43
                0x00, 0x00, 0x80, 0x3f,
                b[3], b[2], b[1], b[0],

                // 44
                0x00, 0x00, 0x80, 0x3f,
                b[3], b[2], b[1], b[0],

                // 45
                0x00, 0x00, 0x80, 0x3f,
                b[3], b[2], b[1], b[0],

                // 46
                0x00, 0x00, 0x80, 0x3f,
                b[3], b[2], b[1], b[0],

                // 47
                0x00, 0x00, 0x80, 0x3f,
                b[3], b[2], b[1], b[0],

                // 48
                0x00, 0x00, 0x00, 0x00,
                b[3], b[2], b[1], b[0],

                // 49
                0x00, 0x00, 0x80, 0x3f,
                b[3], b[2], b[1], b[0],

                // 4a
                0x00, 0x00, 0x80, 0x3f,
                b[3], b[2], b[1], b[0],

                // 4b
                0x00, 0x00, 0x80, 0x3f,
                b[3], b[2], b[1], b[0],

                // 4c
                0x00, 0x00, 0x80, 0x3f,
                b[3], b[2], b[1], b[0],

                // 4d
                0x00, 0x00, 0x80, 0x3f,
                b[3], b[2], b[1], b[0],

                // 4e
                0x00, 0x00, 0x80, 0x3f,
                b[3], b[2], b[1], b[0],

                // 4f
                0x00, 0x00, 0x80, 0x3f,
                b[3], b[2], b[1], b[0],

                // 50
                0x00, 0x00, 0x80, 0x3f,
                b[3], b[2], b[1], b[0],

                // 42 + (41 * 4) = 206

                // 51 - Fire mode
                0x00,
                b[3], b[2], b[1], b[0],

                // 52 - In scope
                0x00,
                b[3], b[2], b[1], b[0],

                // 53 - Primary 1 Ammo Count
                0xff, 0x00,

                // 54 - Secondary 1 Ammo Count
                0xff, 0x00,

                // 55 - Primary 2 Ammo Count
                0xff, 0x00,

                // 56 - Secondary 2 Ammo Count
                0xff, 0x00,

                // 57 - Primary 1 Reserve Ammo Count
                0xff, 0x00,

                // 58 - Secondary 1 Reserve Ammo Count
                0xff, 0x00,

                // 59 - Primary 2 Reserve Ammo Count
                0xff, 0x00,

                // 5a - Secondary 2 Reserve Ammo Count
                0xff, 0x00,

                // 5b - Selected Weapon
                0x00, 0x01, 0x00,
                b[3], b[2], b[1], b[0],

                 // guess 5c
                0x00, 0x00, 0x00,

                // match 5d
                0x00, 0x00, 0x80, 0x3f,

                // 40 + 206 = 246 + 44 = 290

                // match 5e
                0x00, 0x00, 0x00, 0x00,
                b[3], b[2], b[1], b[0],

                // match 5f
                0xff, 0xf0, 0xe4, 0xdf, // 2nd byte 0xe0 = no jetpack, 0xf0 = jetpack
                0x00, 0x00, 0x00, 0x00,
                b[3], b[2], b[1], b[0],

                0x00, 0x00, 0x00, 0x00, // guess 60
                0x00, 0x00, 0x00, 0x00, // guess 61

                // match 62
                b[3], b[2], b[1], b[0],
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x01, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x06, 0x2e,
            };
		}
	}
}
