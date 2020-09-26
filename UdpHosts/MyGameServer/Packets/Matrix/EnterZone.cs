using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using Shared.Udp;

namespace MyGameServer.Packets.Matrix {
    [MatrixMessage(Enums.MatrixPacketType.EnterZone)]
    public class EnterZone {
        [Field]
        public ulong InstanceId;
        [Field]
        public uint ZoneId;
        [Field]
        public ulong ZoneTimestamp;
        [Field]
        public byte PreviewModeFlag;
        [Field]
        public string ZoneOwner;
        [Field]
        [Length(6)]
        public IList<byte> Unk1;
        [Field]
        public byte HotfixLevel;
        [Field]
        public ulong MatchId;
        [Field]
        [Length(5)]
        public IList<byte> Unk2;
        [Field]
        public string ZoneName;
        [Field]
        public byte Unk3;
        [Field]
        [Length(16)]
        public IList<byte> Unk_ZoneTime;
        [Field]
        public ulong Unk4;
        [Field]
        public ulong Unk5;
        [Field]
        public ulong Unk6;
        [Field]
        public ulong Unk7;
        [Field]
        public ulong Unk8;
        [Field]
        public byte Unk9;
        [Field]
        public byte SpectatorModeFlag;
    }
}
