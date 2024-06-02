using System;
using System.Threading;
using AeroMessages.Common;
using AeroMessages.GSS.V66.Character;
using AeroMessages.GSS.V66.Character.Event;
using GameServer.Entities;

namespace GameServer;

public class MovementRelay
{
    private Shard Shard;

    public MovementRelay(Shard shard)
    {
        Shard = shard;
    }

    public void CharacterMovementInput(INetworkClient client, IEntity entity, AeroMessages.GSS.V66.Character.Command.MovementInput input)
    {
        var character = entity as Entities.Character.CharacterEntity;

        // Update our data based on the clients input
        var poseData = input.PoseData;
        var posRotState = poseData.PosRotState;
        character.SetPoseData(poseData, input.ShortTime);

        bool sendJumpActioned = poseData.TimeSinceLastJump < character.TimeSinceLastJump; // Compare the old value before updating 
        character.TimeSinceLastJump = poseData.TimeSinceLastJump;

        character.IsAirborne = poseData.GroundTimePositiveAirTimeNegative < 0;

        var movementStateValue = posRotState.MovementState;
        character.MovementStateContainer.MovementStateValue = (ushort)movementStateValue;

        // Confirm the pose with the client
        var confirmedPose = new ConfirmedPoseUpdate
        {
            PoseData = new MovementPoseData
            {
                ShortTime = input.ShortTime,
                MovementType = MovementDataType.PosRotState,
                WaterLevelAndDesc = poseData.WaterLevelAndDesc,
                PosRotState = new MovementPosRotState
                            {
                                Pos = character.Position,
                                Rot = character.Rotation,
                                MovementState = movementStateValue // ToDo: This was ushort previously!
                            },
                Velocity = character.Velocity,
                JetpackEnergy = poseData.JetpackEnergy,
                GroundTimePositiveAirTimeNegative = poseData.GroundTimePositiveAirTimeNegative, // Somehow affects gravity
                TimeSinceLastJump = poseData.TimeSinceLastJump,
                HaveDebugData = 0
            },
            NextShortTime = unchecked((ushort)(input.ShortTime + 90)) // This value has to be in the future, nobody cares why.
        };
        client.NetChannels[ChannelType.UnreliableGss].SendIAero(confirmedPose, character.EntityId, 0, typeof(Enums.GSS.Character.Events));

        // Forward update to remote clients
        var currentPose = new CurrentPoseUpdate
        {
            Data = new AeroMessages.GSS.V66.CurrentPoseUpdateData
            {
                Flags = 0x00,
                ShortTime = character.MovementShortTime,
                UnkAlwaysPresent = 0x79,
                MovementState = (ushort)character.MovementState,
                Position = character.Position,
                Rotation = character.Rotation,
                Aim = character.AimDirection,
            }
        };
        foreach (var remoteClient in Shard.Clients.Values)
        {
            if (remoteClient.Status.Equals(IPlayer.PlayerStatus.Playing))
            {
                if (sendJumpActioned)
                {
                    remoteClient.NetChannels[ChannelType.UnreliableGss].SendIAero(new JumpActioned { ShortTime = input.ShortTime }, character.EntityId);
                }
    
                remoteClient.NetChannels[ChannelType.UnreliableGss].SendIAero(currentPose, character.EntityId);
            }
        }
    }

    public void VehicleMovementInput(INetworkClient client, IEntity entity, AeroMessages.GSS.V66.Vehicle.Command.MovementInput input)
    {
        var vehicle = entity as Entities.Vehicle.VehicleEntity;
        vehicle.SetPoseData(input);

        if (vehicle.ControllingPlayer?.CharacterEntity != null)
        {
            var character = vehicle.ControllingPlayer.CharacterEntity;
            character.SetPosition(input.Position);
            CharacterMovementInput(client, character, new AeroMessages.GSS.V66.Character.Command.MovementInput()
            {
                ShortTime = client.AssignedShard.CurrentShortTime,
                PoseData = new MovementPoseData()
                {
                    ShortTime = client.AssignedShard.CurrentShortTime,
                    MovementType = MovementDataType.PosRotState,
                    WaterLevelAndDesc = 0,
                    PosRotState = new MovementPosRotState()
                    {
                        Pos = input.Position,
                        Rot = character.Rotation,
                        MovementState = unchecked((short)0xd000)
                    },
                    Velocity = character.Velocity,
                    JetpackEnergy = 0x639c,
                    GroundTimePositiveAirTimeNegative = 0,
                    TimeSinceLastJump = character.TimeSinceLastJump,
                    HaveDebugData = 0
                } 
            });
        }
    }
}