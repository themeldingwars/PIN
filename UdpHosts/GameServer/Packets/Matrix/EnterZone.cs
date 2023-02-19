using GameServer.Enums;
using Shared.Udp;
using System.Collections.Generic;

namespace GameServer.Packets.Matrix;

[MatrixMessage(MatrixPacketType.EnterZone)]
public class EnterZone
{
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
    public IList<byte> Unknown1;

    [Field]
    public byte HotfixLevel;

    [Field]
    public ulong MatchId;

    [Field]
    [Length(5)]
    public IList<byte> Unknown2;

    [Field]
    public string ZoneName;

    [Field]
    public byte Unknown3;

    [Field]
    [Length(16)]
    public IList<byte> Unk_ZoneTime;

    [Field]
    public ulong Unknown4;

    [Field]
    public ulong Unknown5;

    [Field]
    public ulong Unknown6;

    [Field]
    public ulong Unknown7;

    [Field]
    public ulong Unknown8;

    [Field]
    public byte Unknown9;

    [Field]
    public byte SpectatorModeFlag;
}