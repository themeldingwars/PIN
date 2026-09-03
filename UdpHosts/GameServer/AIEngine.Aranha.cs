using System;
using System.Numerics;
using GameServer.Entities.Character;
using GameServer.Entities.Thumper;

namespace GameServer;

/// <summary>
///     The aranha species behavior. Refer to codex/ai-design.md section 4b.
///     Contents: hull climbing, perch selection, pounce leaps at players,
///     random leaps onto thumper hulls, and pack-hunting roles. Each NPC
///     gets its own role and its own speed factor.
///
///     A species file is a partial class of AIEngine. It uses the shared
///     brains, profiles, and movement methods. Engage() stays generic and
///     calls the species hooks by name. Each new species gets its own
///     AIEngine.&lt;Species&gt;.cs file with the same pattern.
/// </summary>
public partial class AIEngine
{
    // The pounce is the aranha leap attack from mid range. The original
    // game had this behavior.
    private const ulong PounceDurationMs = 700;
    private const ulong PounceCooldownMs = 6000;
    private const float PounceMinRange = 5f;
    private const float PounceMaxRange = 11f;
    private const float PounceArcHeight = 2.5f;

    /// <summary>Returns true for a melee type that can climb. Examples: aranha workers and soldiers.</summary>
    private static bool IsAranhaLike(AiProfile profile)
    {
        return profile.CanClimb && profile.PreferredRange <= 4f;
    }

    /// <summary>
    ///     Returns the approach range of one NPC against a thumper hull.
    ///     There are eight range steps. The entity id selects the step.
    ///     Without the steps, all walkers stop on the same circle.
    /// </summary>
    private static float AranhaHullApproachRange(CharacterEntity npc)
    {
        return 0.4f + (((npc.EntityId >> 8) % 8) * 0.15f);
    }

    /// <summary>
    ///     Pack hunting. Each melee NPC gets a role from its entity id.
    ///     About 35 percent attack the target directly. The others move to
    ///     a point on a circle around the target. The point turns slowly
    ///     around the target. Half turn left, and half turn right. Each NPC
    ///     has its own turn speed. The pack then surrounds the target.
    ///     Without the roles, all NPCs follow the target in a narrow group.
    ///     The pounce code adds the leap attacks. The gait output is a
    ///     speed factor for this NPC. Different speeds prevent identical
    ///     movement.
    /// </summary>
    private static Vector3 ComputeMeleePackGoal(CharacterEntity npc, Vector3 targetPosition, float contactRange, float horizontalDistance, ulong currentTime, out float gait)
    {
        gait = 0.8f + (Hash01(npc.EntityId, 0x6A17) * 0.45f);
        var role = Hash01(npc.EntityId, 0xB07E);
        if (role < 0.35f || horizontalDistance >= 20f)
        {
            return targetPosition;
        }

        var spin = role >= 0.675f ? 1f : -1f;
        var wheelSpeed = 0.2f + (Hash01(npc.EntityId, 0x5F1A) * 0.4f);
        var wheelTime = (float)((currentTime % 3600000UL) / 1000.0);
        var slotAngle = (Hash01(npc.EntityId, 0xC12C7E) * 2f * MathF.PI) + (spin * wheelSpeed * wheelTime);
        return targetPosition + new Vector3(
            MathF.Cos(slotAngle) * (contactRange + 0.4f),
            MathF.Sin(slotAngle) * (contactRange + 0.4f),
            0f);
    }

    /// <summary>
    ///     Selects the perch point of one NPC on a hull. The direction
    ///     comes from the approach direction of the NPC, plus a small
    ///     random angle. The height is a random value over the full hull
    ///     height. The swarm then covers the hull from the base to the top.
    /// </summary>
    private static Vector3 ComputePerchOffset(CharacterEntity npc, ThumperEntity thumper, Vector3 toTarget)
    {
        var scale = Math.Max(thumper.Scale, 0.1f);
        var hullHeight = 5f * scale;   // equal to the kinematic capsule height
        var bearing = new Vector3(-toTarget.X, -toTarget.Y, 0f);
        var length = MathF.Sqrt((bearing.X * bearing.X) + (bearing.Y * bearing.Y));
        if (length < 0.01f)
        {
            bearing = new Vector3(1f, 0f, 0f);
            length = 1f;
        }

        bearing /= length;

        // Add a small random angle. NPCs that come from the same direction
        // then get different perch points.
        var jitter = (Hash01(npc.EntityId, 0x51EDA) - 0.5f) * 0.9f;
        var cos = MathF.Cos(jitter);
        var sin = MathF.Sin(jitter);
        bearing = new Vector3((bearing.X * cos) - (bearing.Y * sin), (bearing.X * sin) + (bearing.Y * cos), 0f);

        // A random height over the full climbable hull
        var height = 0.5f + (Hash01(npc.EntityId, 0xA127) * (hullHeight - 1.2f));
        return (bearing * 1.35f * scale) + new Vector3(0f, 0f, height);
    }

    /// <summary>
    ///     Attaches a climber to the thumper hull. Each NPC gets its own
    ///     perch point on the hull. The perch points cover the full hull.
    ///     Without them, all NPCs collect on one narrow band.
    ///     Returns true when the NPC is attached. The movement for this
    ///     think is then complete.
    /// </summary>
    private bool TryAranhaLatch(CharacterEntity npc, BrainState brain, ThumperEntity thumper, Vector3 toTarget, float horizontalDistance, float deltaSeconds, ulong currentTime)
    {
        // Start the attach procedure outside the crowd. The radius is the
        // hull surface radius, scaled to the thumper size, plus the width
        // of approximately two rows of aranha bodies. With a smaller
        // radius, the creature separation blocks the walkers. The walkers
        // then stay outside while the leapers jump over them.
        var latchRadius = (1.35f * Math.Max(thumper.Scale, 0.1f)) + 2.6f;
        if (horizontalDistance >= latchRadius)
        {
            return false;
        }

        LatchOnThumper(npc, brain, thumper, toTarget, deltaSeconds, currentTime);
        TryHitThumper(npc, brain, thumper, horizontalDistance, 2.5f, currentTime);
        BroadcastPoseIfDue(npc, brain, currentTime);
        return true;
    }

    /// <summary>Starts a pounce leap at a player target from mid range. Returns true when the leap starts.</summary>
    private bool TryAranhaPounce(CharacterEntity npc, BrainState brain, CharacterEntity playerTarget, Vector3 toTarget, float horizontalDistance, ulong currentTime)
    {
        if (currentTime < brain.NextPounceAt
            || horizontalDistance < PounceMinRange || horizontalDistance > PounceMaxRange
            || MathF.Abs(toTarget.Z) >= 4f)
        {
            return false;
        }

        StartLeap(npc, brain, playerTarget.Position, 0, currentTime);
        return true;
    }

    /// <summary>
    ///     A climber that comes near a thumper can leap onto the hull. The
    ///     result of a random test selects the leap for half of the NPCs.
    ///     The other half walks. The landing point has a random height on
    ///     the near side of the hull. Returns true when the leap starts.
    /// </summary>
    private bool TryAranhaHullLeap(CharacterEntity npc, BrainState brain, ThumperEntity thumper, Vector3 toTarget, float horizontalDistance, ulong currentTime)
    {
        if (currentTime < brain.NextPounceAt
            || horizontalDistance < 4f || horizontalDistance > PounceMaxRange)
        {
            return false;
        }

        // Each NPC gets its own random result from Hash01. Entity ids
        // increase in steps of 256. A simple parity test gives the same
        // result to a full wave. All NPCs then leap at the same time.
        if (Hash01(npc.EntityId, (uint)(currentTime >> 11)) < 0.5f)
        {
            StartLeap(npc, brain, thumper.Position + ComputePerchOffset(npc, thumper, toTarget), thumper.EntityId, currentTime);
            return true;
        }

        // The NPC walks in this cycle. Do not repeat the random test on
        // each think.
        brain.NextPounceAt = currentTime + PounceCooldownMs;
        return false;
    }

    /// <summary>
    ///     Starts a leap. While the leap is active, only TickLeap controls
    ///     the NPC. The walkableId parameter names the hull for the
    ///     landing. Zero means a landing on the ground.
    /// </summary>
    private void StartLeap(CharacterEntity npc, BrainState brain, Vector3 target, ulong walkableId, ulong currentTime)
    {
        brain.LeapStart = npc.Position;
        brain.LeapTarget = target;
        brain.LeapWalkableId = walkableId;
        brain.LeapStartedAt = currentTime;
        brain.LeapEndsAt = currentTime + PounceDurationMs;
        brain.NextPounceAt = currentTime + PounceCooldownMs + (npc.EntityId % 3000);
        TickLeap(npc, brain, currentTime);
    }

    /// <summary>
    ///     Sets the perch point one time. Then moves the NPC in 3D steps to
    ///     the perch point. At the perch point, the NPC stands and points
    ///     to the hull center.
    /// </summary>
    private void LatchOnThumper(CharacterEntity npc, BrainState brain, ThumperEntity thumper, Vector3 toTarget, float deltaSeconds, ulong currentTime)
    {
        if (!brain.HasPerch)
        {
            brain.PerchOffset = ComputePerchOffset(npc, thumper, toTarget);
            brain.HasPerch = true;
        }

        var perch = thumper.Position + brain.PerchOffset;
        var toPerch = perch - npc.Position;
        var distance = toPerch.Length();
        var inward = SafeNormalize(new Vector3(-brain.PerchOffset.X, -brain.PerchOffset.Y, 0f));

        if (distance < 0.25f)
        {
            SetMovement(npc, inward, StandingMovementState, perch);
        }
        else
        {
            var step = MathF.Min(brain.Profile.FastSpeed * deltaSeconds, distance);
            SetMovement(npc, SafeNormalize(toPerch), RunningMovementState, npc.Position + (SafeNormalize(toPerch) * step));
        }
    }

    /// <summary>
    ///     Moves an active leap forward. The path is a parabolic arc from
    ///     the start point to the target point. The NPC shows the fall
    ///     animation. At the landing: when the target is a hull, the NPC
    ///     attaches at the landing point. If not, the NPC lands on the
    ///     ground. The NPC can attack immediately after the landing.
    /// </summary>
    private void TickLeap(CharacterEntity npc, BrainState brain, ulong currentTime)
    {
        var t = Math.Clamp((currentTime - brain.LeapStartedAt) / (float)PounceDurationMs, 0f, 1f);
        var position = Vector3.Lerp(brain.LeapStart, brain.LeapTarget, t);
        position.Z += PounceArcHeight * 4f * t * (1f - t);

        var aim = SafeNormalize(brain.LeapTarget - npc.Position);
        SetMovement(npc, aim == Vector3.Zero ? npc.AimDirection : aim, FallingMovementState, position);

        if (t >= 1f)
        {
            brain.LeapEndsAt = 0;
            if (brain.LeapWalkableId != 0
                && _shard.Entities.TryGetValue(brain.LeapWalkableId, out var leapTarget)
                && leapTarget is ThumperEntity perchThumper)
            {
                // The NPC lands on the hull. The landing point becomes the perch point.
                brain.PerchOffset = brain.LeapTarget - perchThumper.Position;
                brain.HasPerch = true;
                npc.SetPosition(brain.LeapTarget);
            }
            else
            {
                var landing = brain.LeapTarget;
                landing.Z = _shard.Physics.GetGroundHeight(landing) ?? landing.Z;
                npc.SetPosition(landing);
            }

            brain.LeapWalkableId = 0;
            brain.NextFireAt = currentTime; // The NPC can bite immediately after the landing.
        }

        _shard.Movement.BroadcastCharacterPose(npc);
    }
}
