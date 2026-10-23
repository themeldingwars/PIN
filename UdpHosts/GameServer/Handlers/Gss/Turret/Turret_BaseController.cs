using Aero.Protocol;
using AeroMessages.GSS.Turret.Command;
using AeroMessages.GSS.Turret.View;
using GameServer.Entities.Turret;
using GameServer.Extensions;
using GameServer.Protocol;

namespace GameServer.Handlers.Gss.Turret;

/// <summary>
///     Handlers for the turret base controller view.
/// </summary>
[Typecode(GssTurretView.BaseController)]
public static class Turret_BaseController
{
    [MessageID(GssTurretCommand.PoseUpdate)]
    public static void PoseUpdate(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var poseUpdate = packet.Unpack<PoseUpdate>();
        var turret = shard.Entities[entityId & 0xffffffffffffff00] as TurretEntity;

        if (turret.ControllingPlayer == player)
        {
            turret.Turret_ObserverView.CurrentPoseProp = new CurrentPoseStruct()
                 {
                    Rotation = poseUpdate.Rotation,
                    ShortTime = shard.CurrentShortTime
                 };
        }
    }

    [MessageID(GssTurretCommand.FireBurst)]
    public static void FireBurst(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var fireBurst = packet.Unpack<FireBurst>();
        var turret = shard.Entities[entityId & 0xffffffffffffff00] as TurretEntity;

        turret.SetFireBurst(fireBurst.Time);
    }

    [MessageID(GssTurretCommand.FireEnd)]
    public static void FireEnd(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        var fireEnd = packet.Unpack<FireEnd>();
        var turret = shard.Entities[entityId & 0xffffffffffffff00] as TurretEntity;

        turret.SetFireEnd(fireEnd.Time);
    }

    [MessageID(GssTurretCommand.FireWeaponProjectile)]
    public static void FireWeaponProjectile(NetworkClient client, IShard shard, IPlayer player, ulong entityId, GamePacket packet)
    {
        // TODO: implement - unpack FireWeaponProjectile and route the turret's shot through the WeaponSim.
    }
}
