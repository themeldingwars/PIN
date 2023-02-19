using System;

namespace GameServer.Controllers;

public class MessageIDAttribute : Attribute
{
    public MessageIDAttribute(byte msgId)
    {
        MsgID = msgId;
    }

    public byte MsgID { get; protected set; }
}