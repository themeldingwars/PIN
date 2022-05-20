using GameServer.Enums;
using Shared.Udp;
using System.Collections.Generic;

namespace GameServer.Packets.Matrix
{
    [MatrixMessage(MatrixPacketType.Login)]
    public class Login
    {
        public class UnkStructure1
        {
            [Field]
            public ulong Unk1;

            [Field]
            public string Unk2;
        }

        [Field]
        public byte Unk1;

        [Field]
        public uint ClientVersion;

        [Field]
        public string Unk2;

        [Field]
        public ulong CharacterGUID;

        [Field]
        public uint Unk3;

        [Field]
        public uint Unk4;

        [Field]
        public ushort Unk5;

        [Field]
        public byte Unk6;

        [Field]
        public byte Unk7;

        [Field]
        public byte Unk8;

        [Field]
        public string Red5Sig2; // From Web Requests to ClientAPI

        [Field]
        [ExistsPrefix(typeof(byte), 1)]
        public UnkStructure1 Unk9;

        // TODO: This should read to the end of the packet, it is not necessarily a predefined length
        [Field]
        [Length(370)]
        public IList<byte> OracleTicket;
    }
}