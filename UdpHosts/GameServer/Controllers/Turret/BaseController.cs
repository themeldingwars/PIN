using AeroMessages.GSS.V66.Turret.Command;
using AeroMessages.GSS.V66.Turret.View;
using GameServer.Entities.Turret;
using GameServer.Enums.GSS.Turret;
using GameServer.Extensions;
using GameServer.Packets;
using Serilog;

namespace GameServer.Controllers.Turret;

[ControllerID(Enums.GSS.Controllers.Turret_BaseController)]
public class BaseController : Base
{
    private ILogger _logger;

    public override void Init(INetworkClient client, IPlayer player, IShard shard, ILogger logger)
    {
        _logger = logger;
    }

    [MessageID((byte)Commands.PoseUpdate)]
    public void PoseUpdate(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var poseUpdate = packet.Unpack<PoseUpdate>();
        var turret = client.AssignedShard.Entities[entityId & 0xffffffffffffff00] as TurretEntity;

        if (turret.ControllingPlayer == player)
        {
            turret.Turret_ObserverView.CurrentPoseProp = new CurrentPoseStruct()
                 {
                     Rotation = poseUpdate.Unk1,
                     ShortTime = client.AssignedShard.CurrentShortTime
                 };
        }
    }

    [MessageID((byte)Commands.FireBurst)]
    public void FireBurst(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var fireBurst = packet.Unpack<FireBurst>();
        var turret = client.AssignedShard.Entities[entityId & 0xffffffffffffff00] as TurretEntity;

        turret.SetFireBurst(fireBurst.Time);
    }

    [MessageID((byte)Commands.FireEnd)]
    public void FireEnd(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var fireEnd = packet.Unpack<FireEnd>();
        var turret = client.AssignedShard.Entities[entityId & 0xffffffffffffff00] as TurretEntity;

        turret.SetFireEnd(fireEnd.Time);
    }

    [MessageID((byte)Commands.FireWeaponProjectile)]
    public void FireWeaponProjectile(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // todo
        // var projectile = packet.Unpack<FireWeaponProjectile>();
        // var turret = client.AssignedShard.Entities[entityId & 0xffffffffffffff00] as TurretEntity;
    }
}