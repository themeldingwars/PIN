using System;
using System.Numerics;
using AeroMessages.GSS.Character;
using AeroMessages.GSS.Character.Event;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Protocol;
using Serilog;

namespace GameServer.Systems.MovementRelay;

public class MovementRelay
{
    private const int MaxPoseLogs = 5;

    private static readonly ILogger _logger = Log.ForContext<MovementRelay>();

    private readonly Shard _shard;
    private int _poseLogCount;

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

        if (_poseLogCount < MaxPoseLogs)
        {
            _poseLogCount++;
            _logger.Debug("MovementInput {Entity} movementType={MovementType} hasAim={HasAim} pos={Position} rot={Rotation} aim={Aim} headingPitch={HeadingPitch} headingYaw={HeadingYaw}",
                character.EntityId,
                poseData.MovementType,
                (poseData.MovementType & MovementDataType.Aim) != 0,
                posRotState.Pos,
                posRotState.Rot,
                poseData.Aim,
                (float)input.HeadingPitch,
                (float)input.HeadingYaw);
        }

        character.SetPoseData(poseData, input.ShortTime);

        if ((poseData.MovementType & MovementDataType.Aim) == 0)
        {
            character.Heading = new Vector2((float)input.HeadingPitch, (float)input.HeadingYaw);
            character.SetAimDirection(AimFromHeading(character.Heading.X, character.Heading.Y));
        }

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
        var previousMovestate = character.MovementStateContainer.Movestate;
        character.MovementStateContainer.MovementStateValue = (ushort)movementStateValue;
        if (character.MovementStateContainer.Movestate != previousMovestate)
        {
            _shard.Logger.Debug("Movestate of {Character} changes from {Previous} to {Movestate}", character, previousMovestate, character.MovementStateContainer.Movestate);
        }

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

        if (sendJumpActioned)
        {
            _shard.EntityMan.SendToScoped(character, new JumpActioned { ShortTime = input.ShortTime });
        }

        _shard.EntityMan.SendToScoped(character, currentPose);
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

    /// <summary>
    ///     Resolves a shard entity id to its <see cref="CharacterEntity" /> (the relay owns the shard's entity map;
    ///     lower layers such as the physics engine must not look entities up through the shard themselves).
    /// </summary>
    /// <param name="entityId">The shard entity id to resolve.</param>
    /// <param name="character">Receives the character when the id maps to one; otherwise null.</param>
    /// <returns>True when the id resolves to a <see cref="CharacterEntity" />; false otherwise.</returns>
    public bool TryGetCharacter(ulong entityId, out CharacterEntity character)
    {
        character = null;
        return _shard.Entities.TryGetValue(entityId, out var entity)
            && (character = entity as CharacterEntity) != null;
    }

    /// <summary>
    ///     The shard's current short time (the movement sample/pose clock).
    /// </summary>
    /// <returns>The shard's current short time.</returns>
    public ushort GetShortTime() => _shard.CurrentShortTime;

    /// <summary>
    ///     Relays a pose the physics engine synced back onto a dynamically-simulated character: records a movement
    ///     sample (so the pose can be interpolated/predicted between updates) and forwards a <see cref="CurrentPoseUpdate" />
    ///     to the players scoped to the character. This is the physics counterpart of <see cref="CharacterMovementInput" />,
    ///     which relays client-authoritative poses for kinematic players.
    /// </summary>
    /// <param name="character">The dynamically-simulated character whose pose was synced back.</param>
    /// <param name="shortTime">The short time the pose is stamped with.</param>
    public void RelayDynamicCharacterPose(CharacterEntity character, ushort shortTime)
    {
        // Record a sample so the pose can be interpolated/predicted between updates
        character.RecordMovementSample(new MovementSample
        {
            ShortTime = shortTime,
            Position = character.Position,
            Orientation = character.Orientation,
            Velocity = character.Velocity,
            MovementState = character.MovementState,
        });

        // Forward update to remote clients
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
        _shard.EntityMan.SendToScoped(character, currentPose);
    }

    /// <summary>
    ///     Rebuilds a world-space aim direction from the movement command's heading half-floats. The client
    ///     carries heading as yaw = atan2(dir.X, dir.Y) and pitch = asin(dir.Z), which inverts to
    ///     (cos(pitch) * sin(yaw), cos(pitch) * cos(yaw), sin(pitch)).
    /// </summary>
    /// <param name="headingPitch">The pitch component of the heading.</param>
    /// <param name="headingYaw">The yaw component of the heading.</param>
    /// <returns>A normalized aim direction, or zero when the heading is degenerate.</returns>
    private static Vector3 AimFromHeading(float headingPitch, float headingYaw)
    {
        float cosPitch = MathF.Cos(headingPitch);
        var aim = new Vector3(cosPitch * MathF.Sin(headingYaw), cosPitch * MathF.Cos(headingYaw), MathF.Sin(headingPitch));
        return aim.LengthSquared() > 1e-6f ? Vector3.Normalize(aim) : Vector3.Zero;
    }
}