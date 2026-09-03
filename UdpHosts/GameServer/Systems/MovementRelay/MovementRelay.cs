using AeroMessages.GSS.Character;
using AeroMessages.GSS.Character.Event;
using GameServer.Entities;

namespace GameServer.Systems.MovementRelay;

public class MovementRelay
{
    private readonly Shard _shard;

    public MovementRelay(Shard shard)
    {
        _shard = shard;
    }

    public void CharacterMovementInput(INetworkClient client, IEntity entity, AeroMessages.GSS.Character.Command.MovementInput input)
    {
        var character = entity as Entities.Character.CharacterEntity;

        // Update our data based on the clients input
        var poseData = input.PoseData;
        var posRotState = poseData.PosRotState;
        character.SetPoseData(poseData, input.ShortTime);

        // Record a sample so the pose can be interpolated/predicted between updates
        character.RecordMovementSample(new MovementSample
        {
            ShortTime = input.ShortTime,
            Position = poseData.PosRotState.Pos,
            Orientation = poseData.PosRotState.Rot,
            Velocity = poseData.Velocity,
            MovementState = posRotState.MovementState,
            HorizontalInput = input.HorizontalInput,
            VerticalInput = input.VerticalInput,
            InputFlags = input.InputFlags
        });

        bool sendJumpActioned = poseData.TimeSinceLastJump < character.TimeSinceLastJump; // Compare the old value before updating
        character.TimeSinceLastJump = poseData.TimeSinceLastJump;

        character.IsAirborne = poseData.GroundTimePositiveAirTimeNegative < 0;

        var movementStateValue = posRotState.MovementState;
        character.MovementStateContainer.MovementStateValue = (ushort)movementStateValue;

        // Update with physics
        _shard.Physics.UpdateEntity(character);

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
                                Rot = character.Orientation,
                                MovementState = movementStateValue
                            },
                Velocity = character.Velocity,
                JetpackEnergy = poseData.JetpackEnergy,
                GroundTimePositiveAirTimeNegative = poseData.GroundTimePositiveAirTimeNegative, // Somehow affects gravity
                TimeSinceLastJump = poseData.TimeSinceLastJump,
                HaveDebugData = 0
            },
            NextShortTime = unchecked((ushort)(input.ShortTime + 90)) // This value has to be in the future, nobody cares why.
        };
        client.NetChannels[ChannelType.UnreliableGss].SendMessage(confirmedPose, character.EntityId);

        // Forward update to remote clients
        BroadcastCharacterPose(character, sendJumpActioned, input.ShortTime);
    }

    /// <summary>
    ///     Sends the current pose of a character to all playing clients.
    ///     The AI engine also uses this method for server-moved NPCs.
    /// </summary>
    public void BroadcastCharacterPose(Entities.Character.CharacterEntity character, bool sendJumpActioned = false, ushort jumpShortTime = 0)
    {
        // Keep the physics body in sync for server-moved characters (AI).
        // ProcessMovementInput does this for players, but no code did it
        // for NPCs. NPC hitboxes then stayed at their spawn position.
        _shard.Physics.UpdateEntity(character);

        var currentPose = new CurrentPoseUpdate
        {
            Data = new AeroMessages.GSS.CurrentPoseUpdateData
            {
                Flags = 0x00,
                ShortTime = character.MovementShortTime,
                UnkAlwaysPresent = 0x79,
                MovementState = (ushort)character.MovementState,
                Position = character.Position,
                Rotation = character.Orientation,
                Aim = character.AimDirection,
            }
        };
        foreach (var remoteClient in _shard.Clients.Values)
        {
            if (remoteClient.Status.Equals(IPlayer.PlayerStatus.Playing))
            {
                if (sendJumpActioned)
                {
                    remoteClient.NetChannels[ChannelType.UnreliableGss].SendMessage(new JumpActioned { ShortTime = jumpShortTime }, character.EntityId);
                }

                remoteClient.NetChannels[ChannelType.UnreliableGss].SendMessage(currentPose, character.EntityId);
            }
        }
    }

    public void VehicleMovementInput(INetworkClient client, IEntity entity, AeroMessages.GSS.Vehicle.Command.MovementInput input)
    {
        var vehicle = entity as Entities.Vehicle.VehicleEntity;
        vehicle.SetPoseData(input);

        // Update with physics
        _shard.Physics.UpdateEntity(vehicle);

        if (vehicle.ControllingPlayer?.CharacterEntity != null)
        {
            var character = vehicle.ControllingPlayer.CharacterEntity;
            character.SetPosition(input.Position);
            CharacterMovementInput(client, character, new AeroMessages.GSS.Character.Command.MovementInput()
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
                        Rot = character.Orientation,
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