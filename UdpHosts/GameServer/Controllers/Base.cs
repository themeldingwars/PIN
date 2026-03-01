using System;
using System.Linq;
using System.Reflection;
using GameServer.Extensions;
using GameServer.Packets;
using Serilog;

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
            logger.Warning("---> Unrecognized MsgID for GSS Packet; Controller = {Controller} Entity = 0x{EntityId:X8} MsgID = {MessageId}!", ControllerID, entityId, msgId);
            logger.Warning(">  {PacketData}", BitConverter.ToString(packet.Peek(packet.BytesRemaining).ToArray()).Replace("-", " "));
            return;
        }

        try
        {
            _ = method.Invoke(this, new object[] { client, player, entityId, packet });
        }
        catch (TargetInvocationException e)
        {
            if (e.InnerException != null)
            {
                logger.Error("HandlePacket Caught {ExceptionMessage}", e.InnerException.Message);
                logger.Error("{StackTrace}", e.InnerException.StackTrace);
            }
        }
    }

    protected void LogMissingImplementation<TController>(string endpointName, ulong entityId, GamePacket packet, ILogger logger)
    {
        logger.Warning("Unimplemented Endpoint was called by entity 0x{EntityId:X8}: {ControllerFullName}.{Endpoint}", entityId, typeof(TController).FullName, endpointName);
        logger.Warning(">  {PacketData}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));
    }
}