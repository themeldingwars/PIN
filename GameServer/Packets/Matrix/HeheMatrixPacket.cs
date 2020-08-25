namespace GameServer.Packets.Matrix
{
    public class HeheMatrixPacket : MatrixPacket
    {
        public uint NextSocketId { get; set; }

        public override byte[] ToBytes()
        {
            using (var binaryStream = GetBaseData())
            {
                binaryStream.Write.UInt(NextSocketId);

                binaryStream.ByteOffset = 0;
                return binaryStream.Read.ByteArray((int)binaryStream.Length);
            }
        }
    }
}