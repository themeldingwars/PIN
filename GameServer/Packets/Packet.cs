namespace GameServer.Packets
{
    public abstract class Packet
    {
        public uint SocketId { get; set; }

        public abstract byte[] ToBytes();
    }
}
