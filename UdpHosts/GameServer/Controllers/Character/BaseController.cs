using GameServer.Enums.GSS.Character;
using GameServer.Packets;
using GameServer.Packets.GSS.Character.BaseController;
using KeyFrame = GameServer.Test.GSS.Character.BaseController.KeyFrame;

namespace GameServer.Controllers.Character;

[ControllerID(Enums.GSS.Controllers.Character_BaseController)]
public class BaseController : Base
{
    public override void Init(INetworkClient client, IPlayer player, IShard shard)
    {
        client.NetChans[ChannelType.ReliableGss].SendGSSClass(KeyFrame.Test(player, shard), player.EntityID, msgEnumType: typeof(Events));
        client.NetChans[ChannelType.ReliableGss].SendGSSClass(new Packets.GSS.Character.CombatController.KeyFrame(shard) { PlayerID = player.CharacterID }, player.EntityID, msgEnumType: typeof(Events));
        client.NetChans[ChannelType.ReliableGss].SendGSSClass(new Packets.GSS.Character.LocalEffectsController.KeyFrame(shard) { PlayerID = player.CharacterID }, player.EntityID, msgEnumType: typeof(Events));
        client.NetChans[ChannelType.ReliableGss].SendGSSClass(new Packets.GSS.Character.MissionAndMarkerController.KeyFrame(shard) { PlayerID = player.CharacterID }, player.EntityID, msgEnumType: typeof(Events));
        client.NetChans[ChannelType.ReliableGss].SendGSSClass(new CharacterLoaded(), player.EntityID, msgEnumType: typeof(Events));
    }

    [MessageID((byte)Commands.FetchQueueInfo)]
    public void FetchQueueInfo(INetworkClient client, IPlayer player, ulong entityID, GamePacket packet)
    {
    }

    [MessageID((byte)Commands.PlayerReady)]
    public void PlayerReady(INetworkClient client, IPlayer player, ulong entityID, GamePacket packet)
    {
        player.Ready();
    }

    [MessageID((byte)Commands.MovementInput)]
    public void MovementInput(INetworkClient client, IPlayer player, ulong entityID, GamePacket packet)
    {
        if (packet.BytesRemaining < 64)
        {
            return;
        }

        var pkt = packet.Read<MovementInput>();

        if (!player.CharacterEntity.Alive)
        {
            return;
        }

        player.CharacterEntity.Position = pkt.Position;
        player.CharacterEntity.Rotation = pkt.Rotation;
        player.CharacterEntity.Velocity = pkt.Velocity;
        player.CharacterEntity.AimDirection = pkt.AimDirection;
        player.CharacterEntity.MovementState = (Entities.Character.CharMovement)pkt.State;

        player.CharacterEntity.LastJumpTime ??= pkt.LastJumpTimer;

        //Program.Logger.Warning( "Movement Unk1: {0:X4} {1:X4} {2:X4} {3:X4} {4:X4}", pkt.UnkUShort1, pkt.UnkUShort2, pkt.UnkUShort3, pkt.UnkUShort4, pkt.LastJumpTimer );

        var resp = new ConfirmedPoseUpdate
                   {
                       ShortTime = pkt.ShortTime,
                       UnkByte1 = 1,
                       UnkByte2 = 0,
                       Position = player.CharacterEntity.Position,
                       Rotation = player.CharacterEntity.Rotation,
                       State = (ushort)player.CharacterEntity.MovementState,
                       Velocity = player.CharacterEntity.Velocity,
                       UnkUShort1 = pkt.UnkUShort3,
                       UnkUShort2 = pkt.UnkUShort4, // Somehow affects gravity
                       LastJumpTimer = pkt.LastJumpTimer,
                       UnkByte3 = 0,
                       NextShortTime = unchecked((ushort)(pkt.ShortTime + 90))
                   };

        client.NetChans[ChannelType.UnreliableGss].SendGSSClass(resp, player.EntityID, msgEnumType: typeof(Events));

        if (pkt.LastJumpTimer > player.CharacterEntity.LastJumpTime.Value)
        {
            player.Jump();
        }
    }

    [MessageID((byte)Commands.SetMovementSimulation)]
    public void SetMovementSimulation(INetworkClient client, IPlayer player, ulong entityID, GamePacket packet)
    {
    }

    [MessageID((byte)Commands.BagInventorySettings)]
    public void BagInventorySettings(INetworkClient client, IPlayer player, ulong entityID, GamePacket packet)
    {
    }
}