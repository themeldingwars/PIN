using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using ServerShared;

namespace MyGameServer.Packets.Matrix {
	[MatrixMessage(MatrixPacketType.EnterZone)]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct EnterZone {
        public ulong InstanceId;
        public uint ZoneId;
        public ulong ZoneTimestamp;
        public byte PreviewModeFlag;
        private fixed byte zoneOwner[8];
        public string ZoneOwner {
            get { fixed( byte* t = zoneOwner ) return Utils.ReadFixedString(t, 8); }
            set { fixed( byte* t = zoneOwner ) Utils.WriteFixed(t, Encoding.ASCII.GetBytes(value.Substring(0, Math.Min(8, value.Length)))); }
        }

        private fixed byte unk1[6];
        public byte[] Unk1 {
            get { fixed( byte* t = unk1 ) return Utils.ReadFixed(t, 6); }
            set { fixed( byte* t = unk1 ) Utils.WriteFixed(t, value.AsSpan().Slice(0, Math.Min(6, value.Length))); }
        }
        public byte HotfixLevel;
        public ulong MatchId;
        private fixed byte unk2[5];
        public byte[] Unk2 {
            get { fixed( byte* t = unk2 ) return Utils.ReadFixed(t, 5); }
            set { fixed( byte* t = unk2 ) Utils.WriteFixed(t, value.AsSpan().Slice(0, Math.Min(5, value.Length))); }
        }
        private fixed byte zoneName[24];
        public string ZoneName {
            get { fixed( byte* t = zoneName ) return Utils.ReadFixedString(t, 24); }
            set { fixed( byte* t = zoneName ) Utils.WriteFixed(t, Encoding.ASCII.GetBytes(value.Substring(0, Math.Min(24, value.Length)))); }
        }

        public byte Unk3;
        private fixed byte unk_ZoneTime[16];
        public byte[] Unk_ZoneTime {
            get { fixed( byte* t = unk_ZoneTime ) return Utils.ReadFixed(t, 16); }
            set { fixed( byte* t = unk_ZoneTime ) Utils.WriteFixed(t, value.AsSpan().Slice(0, Math.Min(16, value.Length))); }
        }
        public ulong Unk4;
        public ulong Unk5;
        public ulong Unk6;
        public ulong Unk7;
        public ulong Unk8;
        public byte Unk9;
        public byte SpectatorModeFlag;
    }
}
