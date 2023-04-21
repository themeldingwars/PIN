using GameServer.Extensions;
using GameServer.Packets;
using Serilog;
using System;
using System.Linq;

namespace GameServer.Controllers;

public abstract class Base
{
    protected Base()
    {
        try
        {
            ControllerID = GetType().GetAttribute<ControllerIDAttribute>().ControllerID;
        }
        catch
        {
            throw new MissingMemberException(GetType().FullName, "Missing required ControllerID attribute");
        }
    }

    public Enums.GSS.Controllers ControllerID { get; }

    public abstract void Init(INetworkClient client, IPlayer player, IShard shard, ILogger logger);

    public void HandlePacket(INetworkClient client, IPlayer player, ulong entityId, byte msgId, GamePacket packet, ILogger logger)
    {
        var method = ReflectionUtils.FindMethodsByAttribute<MessageIDAttribute>(this).FirstOrDefault(mi => mi.GetAttribute<MessageIDAttribute>().MsgID == msgId);

        if (method == null)
        {
            logger.Warning("---> Unrecognized MsgID for GSS Packet; Controller = {0} Entity = 0x{1:X8} MsgID = {2}!", ControllerID, entityId, msgId);
            logger.Warning(">  {0}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));
            return;
        }

        _ = method.Invoke(this, new object[] { client, player, entityId, packet });
    }

    protected void LogMissingImplementation<TController>(string endpointName, ulong entityId, GamePacket packet, ILogger logger)
    {
        logger.Warning($"Unimplemented Endpoint was called by entity 0x{{0:X8}}: {typeof(TController).FullName}.{endpointName}", entityId);
        logger.Warning(">  {0}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));
    }
}