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
            logger.Warning("Unhandled message {ControllerName}::{MessageName} (tc-{ControllerTypecode} mid-{MessageId}) from Entity 0x{EntityId:X8}", Enum.GetName(typeof(Enums.GSS.Controllers), ControllerID), GetUnhandledMessageLookup(ControllerID, msgId), (byte)ControllerID, msgId, entityId);
            logger.Warning(">  {PacketData}", BitConverter.ToString(packet.Peek(packet.BytesRemaining).ToArray()).Replace("-", " "));
            return;
        }

        try
        {
            _ = method.Invoke(this, [client, player, entityId, packet]);
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

    private string GetUnhandledMessageLookup(Enums.GSS.Controllers typecode, byte messageId)
    {
        if (typecode > Enums.GSS.Controllers.Character && typecode < Enums.GSS.Controllers.Character_DynamicProjectileView)
        {
            return Enum.GetName(typeof(Enums.GSS.Character.Commands), messageId) ?? "Unknown";
        }
        else if (typecode > Enums.GSS.Controllers.Vehicle && typecode < Enums.GSS.Controllers.Vehicle_MovementView)
        {
            return Enum.GetName(typeof(Enums.GSS.Vehicle.Commands), messageId) ?? "Unknown";
        }
        else if (typecode > Enums.GSS.Controllers.Turret && typecode < Enums.GSS.Controllers.Turret_ObserverView)
        {
            return Enum.GetName(typeof(Enums.GSS.Turret.Commands), messageId) ?? "Unknown";
        }

        return "Unknown";
    }
}