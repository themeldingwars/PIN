using System;
using System.Numerics;
using AeroMessages.GSS;
using AeroMessages.GSS.Character.Event;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.Protocol;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Impact;

public class ForcePushCommand : Command, ICommand
{
    private const uint _pushWindowMs = 1;

    /// <summary>
    ///     How long a pushed NPC body keeps its knockback velocity before the AI steering motor takes over
    ///     again. The def has no duration of its own, so this is a fixed grace period.
    /// </summary>
    private const float _npcPushSeconds = 1.5f;

    private const float _epsilon = 0.001f;

    private readonly ForcePushCommandDef Params;

    public ForcePushCommand(ForcePushCommandDef par)
        : base(par)
    {
        Params = par;
    }

    /// <summary>
    ///     Moves every target away from the point the chain was initiated at, or from the caster, depending on
    ///     impact_position, and drags them towards that point when strength is negative.
    /// </summary>
    /// <param name="context">The executing context</param>
    /// <param name="result">The command result</param>
    public override void Execute(Context context, ref CommandResult result)
    {
        float strength = AbilitySystem.RegistryOp(context.Register, Params.Strength, (Operand)Params.StrengthRegop);
        if (strength == 0f)
        {
            result.SetPass();
            return;
        }

        Vector3 origin = Params.ImpactPosition == 1 ? context.InitBodyPosition : context.Self.Position;

        Logger.Debug("{Command} {CommandId} origin={Origin} impact_position={ParamImpactPosition} initBodyPosition={InitBodyPosition} selfPosition={SelfPosition} strength={Strength}",
            nameof(ForcePushCommand),
            Params.Id,
            origin,
            Params.ImpactPosition,
            context.InitBodyPosition,
            context.Self.Position,
            strength);

        foreach (IAptitudeTarget target in context.Targets)
        {
            if (target is not CharacterEntity character)
            {
                continue;
            }

            PushCharacter(context, character, origin, strength);
        }

        result.SetPass();
    }

    private static Vector3 WithLoft(Vector3 direction, float loft)
    {
        float climb = loft * new Vector2(direction.X, direction.Y).Length();
        float z = direction.Z < climb && direction.Z > -climb ? climb : direction.Z;
        Vector3 raised = new(direction.X, direction.Y, z);

        return raised.LengthSquared() <= _epsilon ? Vector3.UnitZ : Vector3.Normalize(raised);
    }

    /// <summary>
    ///     Works out the push velocity for one target and hands it to whichever mover owns that character: the
    ///     physics engine for server-authoritative bodies, the client for player controlled ones. A negative
    ///     strength drags the target towards the origin instead of shoving it away.
    /// </summary>
    /// <param name="context">The executing context</param>
    /// <param name="character">The target to push</param>
    /// <param name="origin">The point the target is pushed away from, or pulled towards for negative strength</param>
    /// <param name="strength">The regop resolved push strength, negative meaning a pull</param>
    private void PushCharacter(Context context, CharacterEntity character, Vector3 origin, float strength)
    {
        if (!character.TryGetInterpolatedPose(context.CurrentTime, null, out var target, out _, out _))
        {
            target = character.Position;
        }

        Vector3 displacement = target - origin;

        if (displacement.LengthSquared() <= _epsilon)
        {
            displacement = Vector3.UnitZ;
        }

        Vector3 direction = Vector3.Normalize(displacement);

        // falloff is the distance over which the shove dies out, and zero means it does not fall off at all.
        float factor = 1f;
        if (Params.Falloff > 0f)
        {
            factor = Math.Clamp(1f - (displacement.Length() / Params.Falloff), 0f, 1f);
            if (factor <= 0f)
            {
                Logger.Debug("{Command} {CommandId} skipped {Entity}: {Distance}m from the origin is past the {ParamFalloff}m falloff", nameof(ForcePushCommand), Params.Id, character.EntityId, displacement.Length(), Params.Falloff);
                return;
            }
        }

        float speed = Math.Abs(strength) * factor;

        Vector3 travel = direction * Math.Sign(strength);
        Vector3 velocity = WithLoft(travel, Params.Loft) * speed;

        if (character.IsPlayerControlled && character.Player != null)
        {
            SendForcedMovement(context, character, velocity);
            return;
        }
        else
        {
            if (context.Shard.Physics.ApplyPushVelocity(character, velocity, _npcPushSeconds))
            {
                Logger.Debug("{Command} {CommandId} pushed {Entity} target={TargetPosition} displacement={Displacement} to {Velocity}",
                    nameof(ForcePushCommand),
                    Params.Id,
                    character.EntityId,
                    target,
                    displacement,
                    velocity);
                return;
            }
        }

        Logger.Debug("{Command} {CommandId} cannot push {Entity}: no dynamic body and not player controlled", nameof(ForcePushCommand), Params.Id, character.EntityId);
    }

    /// <summary>
    ///     Player characters ride kinematic bodies, which have no mass for an impulse to act on, so the push has
    ///     to be handed to the client.
    /// </summary>
    /// <param name="context">The executing context</param>
    /// <param name="character">The player to tell</param>
    /// <param name="velocity">The velocity to set the client's character to</param>
    private void SendForcedMovement(Context context, CharacterEntity character, Vector3 velocity)
    {
        var message = new ForcedMovement
        {
            Data = new ForcedMovementData
            {
                Type = 5,
                CommandId = Params.Id,
                Params5 = new ForcedMovementType5Params
                {
                    Velocity = velocity,
                    Time1 = context.CurrentTime,
                    Time2 = context.CurrentTime + _pushWindowMs,
                    Unk2 = 0,
                },
            },
            ShortTime = context.Shard.CurrentShortTime,
        };

        character.Player.NetChannels[ChannelType.ReliableGss].SendMessage(message, character.EntityId);

        Logger.Debug("{Command} {CommandId} sent ForcedMovement type 5 to {Entity} velocity={Velocity}", nameof(ForcePushCommand), Params.Id, character.EntityId, velocity);
    }
}