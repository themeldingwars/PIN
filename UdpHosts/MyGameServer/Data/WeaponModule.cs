using MyGameServer.Packets.GSS.Character.BaseController;

namespace MyGameServer.Data
{
    public class WeaponModule
    {
        public WeaponModule(uint id)
        {
            ID = id;
            UnkBytes = new byte[] { 0xff, 0x00, 0x00 };
        }

        public uint ID { get; set; }
        public byte[] UnkBytes { get; protected set; }

        public static implicit operator KeyFrame.WeaponModule(WeaponModule o)
        {
            return new KeyFrame.WeaponModule { ID = o.ID, Unk = o.UnkBytes };
        }
    }
}