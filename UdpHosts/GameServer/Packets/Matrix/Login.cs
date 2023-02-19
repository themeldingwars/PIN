using GameServer.Enums;
using Shared.Udp;
using System.Collections.Generic;

namespace GameServer.Packets.Matrix;

[MatrixMessage(MatrixPacketType.Login)]
public class Login
{
    [Field]
    public byte Unknown1;

    [Field]
    public uint ClientVersion;

    [Field]
    public string Unknown2;

    [Field]
    public ulong CharacterGUID;

    [Field]
    public uint Unknown3;

    [Field]
    public uint Unknown4;

    [Field]
    public ushort Unknown5;

    [Field]
    public byte Unknown6;

    [Field]
    public byte Unknown7;

    [Field]
    public byte Unknown8;

    [Field]
    public string Red5Sig2; // From Web Requests to ClientAPI

    [Field]
    [ExistsPrefix(typeof(byte), 1)]
    public UnknownStructure1 Unknown9;

    // TODO: This should read to the end of the packet, it is not necessarily a predefined length
    [Field]
    [Length(370)]
    public IList<byte> OracleTicket;

    public class UnknownStructure1
    {
        [Field]
        public ulong Unknown1;

        [Field]
        public string Unknown2;
    }
}