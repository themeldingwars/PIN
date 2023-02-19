using AeroMessages.GSS.V66.Character;
using AeroMessages.GSS.V66.Character.Command;
using GameServer.Entities.Character;
using GameServer.Enums.GSS.Character;
using GameServer.Extensions;
using GameServer.Packets;
using GameServer.Packets.GSS.Character.BaseController;
using System;
using ConfirmedPoseUpdate = AeroMessages.GSS.V66.Character.Event.ConfirmedPoseUpdate;
using KeyFrame = GameServer.Test.GSS.Character.BaseController.KeyFrame;

namespace GameServer.Controllers.Character;

[ControllerID(Enums.GSS.Controllers.Character_BaseController)]
public class BaseController : Base
{
    public override void Init(INetworkClient client, IPlayer player, IShard shard)
    {
        client.NetChannels[ChannelType.ReliableGss].SendGSSClass(KeyFrame.Test(player, shard), player.EntityId, msgEnumType: typeof(Events));
        client.NetChannels[ChannelType.ReliableGss].SendGSSClass(new Packets.GSS.Character.CombatController.KeyFrame(shard) { PlayerID = player.CharacterId }, player.EntityId, msgEnumType: typeof(Events));
        client.NetChannels[ChannelType.ReliableGss].SendGSSClass(new Packets.GSS.Character.LocalEffectsController.KeyFrame(shard) { PlayerID = player.CharacterId }, player.EntityId, msgEnumType: typeof(Events));
        client.NetChannels[ChannelType.ReliableGss].SendGSSClass(new Packets.GSS.Character.MissionAndMarkerController.KeyFrame(shard) { PlayerID = player.CharacterId }, player.EntityId, msgEnumType: typeof(Events));
        client.NetChannels[ChannelType.ReliableGss].SendGSSClass(new CharacterLoaded(), player.EntityId, msgEnumType: typeof(Events));
    }

    [MessageID((byte)Commands.FetchQueueInfo)]
    public void FetchQueueInfo(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // ToDo: Implement BaseController.FetchQueueInfo
        LogMissingImplementation<BaseController>(nameof(FetchQueueInfo), entityId, packet);
    }

    [MessageID((byte)Commands.PlayerReady)]
    public void PlayerReady(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        player.Ready();
    }

    [MessageID((byte)Commands.MovementInput)]
    public void MovementInput(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // ToDo: This currently only handles PosRotState inputs, logic needs to be added for the other MovementDataTypes

        if (packet.BytesRemaining < 64)
        {
            return;
        }


        var movementInput = packet.Unpack<MovementInput>();

        if (!player.CharacterEntity.Alive)
        {
            return; // can't move if you're dead (or at least shouldn't o.o")
        }

        var poseData = movementInput.PoseData;
        var posRotState = poseData.PosRotState;


        player.CharacterEntity.Position = posRotState.Pos;
        player.CharacterEntity.Rotation = posRotState.Rot;
        player.CharacterEntity.Velocity = poseData.Velocity;
        player.CharacterEntity.AimDirection = poseData.Aim;


        var movementStateValue = posRotState.MovementState;
        player.CharacterEntity.MovementStateContainer.MovementState = (CharMovementState)movementStateValue;

        if (player.CharacterEntity.MovementStateContainer.InvalidFlags != CharMovementState.None)
        {
            Log.Error($"Unmapped {nameof(CharMovementState)} encountered! \n{player.CharacterEntity.MovementStateContainer}");
        }


        var timeSinceLastJumpValue = poseData.TimeSinceLastJump;
        player.CharacterEntity.TimeSinceLastJump ??=
            timeSinceLastJumpValue >= 0 ? Convert.ToUInt16(timeSinceLastJumpValue) : throw new ArgumentOutOfRangeException($"{nameof(poseData.TimeSinceLastJump)} is <0, but we're only allowing >=0. This is bad!");

        //Program.Logger.Warning( "Movement Unknown1: {0:X4} {1:X4} {2:X4} {3:X4} {4:X4}", pkt.UnkUShort1, pkt.UnkUShort2, pkt.UnkUShort3, pkt.UnkUShort4, pkt.LastJumpTimer );

        var resp = new ConfirmedPoseUpdate
                   {
                       PoseData = new MovementPoseData
                                  {
                                      MovementUnk1 = movementInput.ShortTime, //ShortTime
                                      MovementType = MovementDataType.PosRotState,
                                      MovementUnk3 = 0, // ToDo: Find out why this has to be 0; What does it control?
                                      PosRotState = new MovementPosRotState
                                                    {
                                                        Pos = player.CharacterEntity.Position,
                                                        Rot = player.CharacterEntity.Rotation,
                                                        MovementState = (short)player.CharacterEntity.MovementStateContainer.MovementState // ToDo: This was ushort previously!
                                                    },
                                      Velocity = player.CharacterEntity.Velocity,
                                      JetpackEnergy = poseData.JetpackEnergy,
                                      GroundTimePositiveAirTimeNegative = poseData.GroundTimePositiveAirTimeNegative, // Somehow affects gravity
                                      TimeSinceLastJump = poseData.TimeSinceLastJump,
                                      HaveMoreData = 0 // this has to be 0 for now, according to AeroMessages: "Looks like 1 more byte should be read here and based on it a lot of more shit can happen"
                                      // and we don't quite know yet what "a lot of more shit" looks like so we don't flip it to 1 for the time being
                                  },
                       Unk = unchecked((ushort)(movementInput.ShortTime + 90)) // NextShortTime
                   };

        // ToDo: Set "Aim" property of response if the input had the respective flag
        // ToDo: Handle JetPackEnergy changes / add to CharacterEntity class

        client.NetChannels[ChannelType.UnreliableGss].SendIAero(resp, entityId, typeof(Events));

        if (player.CharacterEntity.TimeSinceLastJump.HasValue && poseData.TimeSinceLastJump > player.CharacterEntity.TimeSinceLastJump.Value)
        {
            player.Jump();
        }
    }

    [MessageID((byte)Commands.SetMovementSimulation)]
    public void SetMovementSimulation(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // ToDo: Implement BaseController.SetMovementSimulation
        LogMissingImplementation<BaseController>(nameof(SetMovementSimulation), entityId, packet);
    }

    [MessageID((byte)Commands.BagInventorySettings)]
    public void BagInventorySettings(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // ToDo: Implement BaseController.BagInventorySettings
        LogMissingImplementation<BaseController>(nameof(BagInventorySettings), entityId, packet);

        var bagInventorySettings = packet.Unpack<BagInventorySettings>();
    }

    [MessageID((byte)Commands.SetSteamUserId)]
    public void SetSteamUserId(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var setSteamIdPacket = packet.Unpack<SetSteamUserId>();
        player.SteamUserId = setSteamIdPacket.SteamUserId;
        Log.Verbose("Entity {0:x8} Steam user id (Aero): {1}", entityId, player.SteamUserId);
        //var conventional = packet.Read<SetSteamIdRequest>();
        //Log.Verbose("Packet Data: {0}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));
        //Log.Verbose("Entity {0:x8} Steam user id (conventional): {1}", entityId, conventional.SteamId);
    }
}