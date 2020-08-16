namespace GameServer.Packets.Matrix
{
    public class KissMatrixPacket : MatrixPacket
    {
        public uint NextSocketId { get; set; }
        public uint StreamingProtocolVersion { get; set; }

        public override byte[] ToBytes()
        {
            using (var binaryStream = GetBaseData())
            {
                binaryStream.Write.UInt(NextSocketId);
                binaryStream.Write.UInt(StreamingProtocolVersion);

                binaryStream.ByteOffset = 0;
                return binaryStream.Read.ByteArray((int)binaryStream.Length);
            }
        }
    }
}