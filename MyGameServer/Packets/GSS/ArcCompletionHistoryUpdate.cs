using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using ServerShared;

namespace MyGameServer.Packets.GSS {
	[GSSMessage((byte)GenericEvents.ArcCompletionHistoryUpdate)]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct ArcCompletionHistoryUpdate : IWritableStruct {
		public static ArcCompletionHistoryUpdate Parse(Packet packet) {
			var ret = new ArcCompletionHistoryUpdate();

			var num = packet.Read<byte>();
			ret.Entries = new Entry[num];

			for( var i=0; i<num; i++ ) {
				ret.Entries[i] = packet.Read<Entry>();
			}

			return ret;
		}

		public Entry[] Entries;

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Entry {
			public uint Unk1; // Sort? Position in GUI?
			public uint Unk2; // Extra?
			public uint Unk3; // Status?
			public uint Unk4; // Arc ID?

			public Entry(uint u1, uint u2, uint u3, uint u4) {
				Unk1 = u1;
				Unk2 = u2;
				Unk3 = u3;
				Unk4 = u4;
			}
		}

		public Memory<byte> Write(  ) {
			Memory<byte> mem = new byte[1 + (4*Entries.Length)];

			Utils.WriteStruct(Entries.Length);
			
			for(var i=0; i<Entries.Length; i++) {
				Utils.WriteStruct(mem.Slice(1 + (i * 4)), Entries[i]);
			}

			return mem;
		}

		public Memory<byte> WriteBE( ) {
			Memory<byte> mem = new byte[1 + (4 * Entries.Length)];

			Utils.WriteStructBE(Entries.Length);

			for( var i = 0; i < Entries.Length; i++ ) {
				Utils.WriteStructBE(mem.Slice(1 + (i * 4)), Entries[i]);
			}

			return mem;
		}
	}
}