using Bitter;

namespace GameServer.Packets.Matrix
{
    public abstract class MatrixPacket : Packet
    {
        public byte[] Type { get; set; }

        protected BinaryStream GetBaseData()
        {
            var binaryStream = new BinaryStream();

            binaryStream.Write.UInt(SocketId);
            binaryStream.Write.ByteArray(Type);

            return binaryStream;
        }
    }
}