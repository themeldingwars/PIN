using System;

namespace GameServer.Packets
{
    public class GSSMessageAttribute : Attribute
    {
        public GSSMessageAttribute(byte mID)
        {
            ControllerID = null;
            EntityID = null;
            MsgID = mID;
        }

        public GSSMessageAttribute(Enums.GSS.Controllers cID, byte mID)
        {
            ControllerID = cID;
            EntityID = null;
            MsgID = mID;
        }

        public Enums.GSS.Controllers? ControllerID { get; protected set; }
        public ulong? EntityID { get; protected set; }
        public byte MsgID { get; protected set; }
    }
}