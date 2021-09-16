using MyGameServer.Enums;
using Shared.Udp;
using System.Collections.Generic;

namespace MyGameServer.Packets.Matrix
{
    [MatrixMessage(MatrixPacketType.EnterZone)]
    public class EnterZone
    {
        [Field] public byte HotfixLevel;

        [Field] public ulong InstanceId;

        [Field] public ulong MatchId;

        [Field] public byte PreviewModeFlag;

        [Field] public byte SpectatorModeFlag;

        [Field] [Length(16)] public IList<byte> Unk_ZoneTime;

        [Field] [Length(6)] public IList<byte> Unk1;

        [Field] [Length(5)] public IList<byte> Unk2;

        [Field] public byte Unk3;

        [Field] public ulong Unk4;

        [Field] public ulong Unk5;

        [Field] public ulong Unk6;

        [Field] public ulong Unk7;

        [Field] public ulong Unk8;

        [Field] public byte Unk9;

        [Field] public uint ZoneId;

        [Field] public string ZoneName;

        [Field] public string ZoneOwner;

        [Field] public ulong ZoneTimestamp;
    }
}