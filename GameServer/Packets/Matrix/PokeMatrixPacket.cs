namespace GameServer.Packets.Matrix
{
    public class PokeMatrixPacket : MatrixPacket
    {
        public ushort ProtocolVersion { get; set; }

        public override byte[] ToBytes()
        {
            using (var binaryStream = GetBaseData())
            {
                binaryStream.Write.UShort(ProtocolVersion);

                binaryStream.ByteOffset = 0;
                return binaryStream.Read.ByteArray((int)binaryStream.Length);
            }
        }
    }
}