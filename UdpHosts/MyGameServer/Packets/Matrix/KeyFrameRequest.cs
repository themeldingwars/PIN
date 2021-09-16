using MyGameServer.Enums;
using Shared.Udp;
using System.Collections.Generic;

namespace MyGameServer.Packets.Matrix
{
    [MatrixMessage(MatrixPacketType.KeyframeRequest)]
    public class KeyFrameRequest
    {
        [ExistsPrefix(typeof(byte), 1)] [Field] [LengthPrefixed(typeof(byte))]
        public IList<RequestByEntity> EntityRequests;

        [ExistsPrefix(typeof(byte), 1)] [Field] [LengthPrefixed(typeof(byte))]
        public IList<ushort> RefRequests;

        public class RequestByEntity
        {
            [Field] public uint Checksum;

            [Field] public byte ControllerID;

            [Field] [Length(7)] public IList<byte> EntityID;

            [Field] public ushort RefID;

            [Field] public byte Unk2;
        }
    }
}