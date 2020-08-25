namespace GameServer.Packets.Matrix
{
    public class HuggMatrixPacket : MatrixPacket
    {
        public ushort GameServerPort { get; set; }

        public ushort SequenceStart { get; set; }

        public override byte[] ToBytes()
        {
            using (var binaryStream = GetBaseData())
            {
                binaryStream.Write.UShort(GameServerPort);
                binaryStream.Write.UShort(SequenceStart);

                binaryStream.ByteOffset = 0;
                return binaryStream.Read.ByteArray((int)binaryStream.Length);
            }
        }
    }
}