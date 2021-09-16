using MyGameServer.Packets.GSS.Character.BaseController;

namespace MyGameServer.Data
{
    public class AbilityModule
    {
        public uint SdbID { get; set; }
        public byte SlotIDX { get; set; }

        public static AbilityModule Load(uint id, byte slot)
        {
            return new AbilityModule { SdbID = id, SlotIDX = slot };
        }

        public static implicit operator KeyFrame.Ability(AbilityModule o)
        {
            return new KeyFrame.Ability { Slot = o.SlotIDX, ID = o.SdbID, UnkByte1 = 0 };
        }
    }
}