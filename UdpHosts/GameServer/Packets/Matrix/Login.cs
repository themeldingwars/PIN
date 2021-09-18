using GameServer.Enums;
using Shared.Udp;
using System.Collections.Generic;

namespace GameServer.Packets.Matrix
{
    [MatrixMessage(MatrixPacketType.Login)]
    public class Login
    {
        [Field] public ulong CharacterGUID;

        [Field] public ushort ClientVersion;

        [Field] [Length(370)] public IList<byte> Red5Sig1; // From Web Requests to ClientAPI

        [Field] public string Red5Sig2; // From Web Requests to ClientAPI

        [Field] public byte Unk1;

        [Field] [Length(3)] public IList<byte> Unk2;

        [Field] [Length(13)] public IList<byte> Unk3;
    }
}