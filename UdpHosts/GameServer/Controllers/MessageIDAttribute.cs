using System;

namespace GameServer.Controllers;

public class MessageIDAttribute : Attribute
{
    public MessageIDAttribute(byte msgID)
    {
        MsgID = msgID;
    }

    public byte MsgID { get; protected set; }
}