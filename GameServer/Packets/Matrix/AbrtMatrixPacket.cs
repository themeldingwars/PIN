namespace GameServer.Packets.Matrix
{
    public class AbrtMatrixPacket : MatrixPacket
    {
        public override byte[] ToBytes()
        {
            using (var binaryStream = GetBaseData())
            {
                binaryStream.ByteOffset = 0;
                return binaryStream.Read.ByteArray((int)binaryStream.Length);
            }
        }
    }
}