using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Systems.CharacterLifecycle;
using GameServer.Systems.SystemEvents;
using Serilog;

namespace GameServer.Systems.Ai;

/// <summary>
///     Server side NPC AI. Tracks every spawned mob, gives it a target, walks it around
///     and lets it shoot back. Replaces the previous no-op <c>AIEngine</c> stub and is
///     ticked from <c>Shard.Tick</c> alongside the other systems.
/// </summary>
/// <remarks>
///     The decision making lives in <see cref="AiBrain" />, which knows nothing about
///     entities. This class is the boring part: target selection, line of sight, applying
///     movement to the entity and physics body, broadcasting the pose to clients and
///     routing damage through <c>IShard.Damage</c>.
/// </remarks>
public class AiEngine
{
    /// <summary>How much further than the aggro radius an already engaged NPC will chase.</summary>
    private const float _chaseSlackMultiplier = 1.5f;

    /// <summary>Raise the trace above the feet so the ground probe does not start inside the body.</summary>
    private const float _groundProbeUp = 1.5f;

    /// <summary>
    ///     How far below the feet the ground probe reaches before we give up. Large so the
    ///     mob follows the terrain down ledges and slopes instead of floating off them.
    /// </summary>
    private const float _groundProbeDown = 100f;

    /// <summary>
    ///     Height above the feet used for the wall/obstacle ray. Keeping the ray off the
    ///     ground stops it from grazing the terrain the mob is standing on and reporting a
    ///     false "blocked" every step.
    /// </summary>
    private const float _wallCheckHeight = 1f;

    /// <summary>Chest height, used for both the line of sight trace and the aim direction.</summary>
    private const float _eyeHeight = 1.4f;

    private static readonly short _movementStateIdle = (short)((ushort)Movestate.Standing << 8);
    private static readonly short _movementStateRunning = (short)(((ushort)Movestate.Running << 8) | (ushort)MovementFlags.Movement);

    private readonly ConcurrentDictionary<ulong, NpcBrain> _brains = new();
    private readonly ILogger _logger;
    private readonly IAiRules _rules;
    private readonly IAiHostility _hostility;
    private readonly IAiAttackFeedback _feedback;
    private readonly IAiMonsterStats _monsterStats;
    private readonly IShard _shard;
    private ulong _lastPerceptionAt;
    private ulong _lastMovementAt;

    public AiEngine(
        IShard shard,
        IEventBus eventBus,
        IAiRules rules = null,
        IAiHostility hostility = null,
        IAiAttackFeedback feedback = null,
        IAiMonsterStats monsterStats = null)
    {
        _shard = shard ?? throw new ArgumentNullException(nameof(shard));
        _rules = rules ?? new StandardAiRules();
        _hostility = hostility ?? new FactionAiHostility();
        _feedback = feedback ?? new HitFeedbackAttackFeedback(shard);
        _monsterStats = monsterStats ?? new SdbAiMonsterStats();
        _logger = shard.Logger?.ForContext<AiEngine>() ?? Log.ForContext<AiEngine>();

        // The bus is injected like every other system's: IShard does not expose it.
        eventBus?.Subscribe<EntityDamagedEvent>(OnEntityDamaged);
        eventBus?.Subscribe<CharacterDiedEvent>(OnCharacterDied);
    }

    /// <summary>Runtime kill switch, toggled by the <c>ai</c> chat/admin command.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Number of NPCs currently being simulated.</summary>
    public int TrackedCount => _brains.Count;

    /// <summary>Behaviour state of a tracked NPC, or null when it is not tracked.</summary>
    public AiBrainState? GetState(ulong entityId)
    {
        return _brains.TryGetValue(entityId, out var npc) ? new AiBrainState?(npc.Brain.State) : null;
    }

    /// <summary>Whether this entity currently has an AI brain.</summary>
    public bool IsTracked(ulong entityId) => _brains.ContainsKey(entityId);

    /// <summary>Snapshot of the entity ids currently being simulated. Mainly for debugging.</summary>
    public IReadOnlyCollection<ulong> GetTrackedEntityIds() => _brains.Keys.ToArray();

    /// <summary>Starts simulating an NPC. Player controlled characters are ignored.</summary>
    public bool Register(CharacterEntity npc)
    {
        if (npc == null || npc.IsPlayerControlled)
        {
            return false;
        }

        var (normalSpeed, fastSpeed) = _monsterStats.GetSpeeds(npc.StaticInfo.CharacterTypeId);
        var brain = new NpcBrain
        {
            EntityId = npc.EntityId,
            Entity = npc,
            Home = npc.Position,
            Brain = new AiBrain(_rules, _shard.CurrentTimeLong),
            MoveSpeed = AiSpeeds.Resolve(normalSpeed, _rules.DefaultMoveSpeed, _rules),
            ChaseSpeed = AiSpeeds.Resolve(fastSpeed, _rules.DefaultChaseSpeed, _rules),
        };

        return _brains.TryAdd(npc.EntityId, brain);
    }

    /// <summary>Stops simulating an NPC. Safe to call for unknown ids.</summary>
    public bool Unregister(ulong entityId) => _brains.TryRemove(entityId, out _);

    /// <summary>Forces an NPC to engage whoever shot it.</summary>
    public void Aggro(ulong npcEntityId, ulong attackerEntityId)
    {
        if (attackerEntityId == 0 || !_brains.TryGetValue(npcEntityId, out var npc))
        {
            return;
        }

        npc.TargetId = attackerEntityId;
        npc.Brain.Aggro(_shard.CurrentTimeLong);
    }

    /// <summary>Drops every brain. Used when a zone is torn down.</summary>
    public void Clear() => _brains.Clear();

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        if (_brains.IsEmpty || ct.IsCancellationRequested)
        {
            return;
        }

        if (!Enabled || !_rules.Enabled)
        {
            return;
        }

        if (currentTime < _lastMovementAt + (ulong)_rules.MovementIntervalMs)
        {
            return;
        }

        // deltaTime is the time since the previous shard tick, but we only act once per
        // movement interval, so step by the time actually elapsed since we last moved.
        ulong elapsedMs = _lastMovementAt == 0 ? (ulong)_rules.MovementIntervalMs : currentTime - _lastMovementAt;
        _lastMovementAt = currentTime;

        bool perceive = currentTime >= _lastPerceptionAt + (ulong)_rules.PerceptionIntervalMs;
        if (perceive)
        {
            _lastPerceptionAt = currentTime;
        }

        foreach (var entry in _brains)
        {
            UpdateBrain(entry.Value, elapsedMs, currentTime, perceive);
        }
    }

    private void OnEntityDamaged(EntityDamagedEvent evt)
    {
        if (evt.Target == null || evt.Source == null || evt.Source.EntityId == 0)
        {
            return;
        }

        // Only NPCs are tracked, so a player taking damage is a no-op here.
        Aggro(evt.Target.EntityId, evt.Source.EntityId);
    }

    private void OnCharacterDied(CharacterDiedEvent evt)
    {
        if (evt.Target != null && _brains.TryGetValue(evt.Target.EntityId, out var npc))
        {
            npc.Brain.OnDeath();
            npc.TargetId = 0;
        }
    }

    private void UpdateBrain(NpcBrain npc, ulong elapsedMs, ulong currentTime, bool perceive)
    {
        var entity = npc.Entity;
        if (entity == null || entity.IsPlayerControlled ||
            !_shard.Entities.TryGetValue(npc.EntityId, out var registered) ||
            !ReferenceEquals(registered, entity))
        {
            _brains.TryRemove(npc.EntityId, out _);
            return;
        }

        if (npc.Brain.State == AiBrainState.Dead || !entity.IsAlive)
        {
            npc.Brain.OnDeath();
            npc.TargetId = 0;
            return;
        }

        if (perceive)
        {
            RefreshTarget(npc);
        }

        CharacterEntity target = null;
        bool targetAlive = false;
        if (npc.TargetId != 0 &&
            _shard.Entities.TryGetValue(npc.TargetId, out var targetEntity) &&
            targetEntity is CharacterEntity targetCharacter)
        {
            target = targetCharacter;
            targetAlive = targetCharacter.IsAlive;
        }
        else
        {
            npc.TargetId = 0;
        }

        float distanceToTarget = target != null ? AiVectors.HorizontalDistance(entity.Position, target.Position) : float.MaxValue;
        bool visible = targetAlive && HasLineOfSight(entity, target);

        var perception = new AiPerception(
            npc.TargetId,
            targetAlive,
            visible,
            distanceToTarget,
            AiVectors.HorizontalDistance(entity.Position, npc.Home),
            currentTime);

        var decision = npc.Brain.Decide(perception);

        if (!npc.Brain.WantsTarget)
        {
            npc.TargetId = 0;
        }

        if (decision.Attack && targetAlive)
        {
            ResolveAttack(npc, target);
        }

        ApplyDecision(npc, decision, elapsedMs, target);
    }

    private void RefreshTarget(NpcBrain npc)
    {
        if (npc.Brain.State == AiBrainState.Dead)
        {
            return;
        }

        // Keep the current target while it still exists. Re-scanning every perception pass
        // would make a mob ping pong between two players standing side by side.
        if (npc.TargetId != 0 && _shard.Entities.ContainsKey(npc.TargetId))
        {
            return;
        }

        npc.TargetId = 0;

        float radius = npc.Brain.WantsTarget ? _rules.AggroRadius * _chaseSlackMultiplier : _rules.AggroRadius;
        var origin = npc.Entity.Position;

        ulong bestId = 0;
        float bestDistance = float.MaxValue;

        foreach (var client in _shard.Clients.Values)
        {
            var candidate = client?.CharacterEntity;
            if (candidate == null || !candidate.IsAlive)
            {
                continue;
            }

            if (!_hostility.IsHostile(npc.Entity, candidate))
            {
                continue;
            }

            float distance = AiVectors.HorizontalDistance(origin, candidate.Position);
            if (distance > radius || distance >= bestDistance)
            {
                continue;
            }

            bestDistance = distance;
            bestId = candidate.EntityId;
        }

        if (bestId != 0)
        {
            npc.TargetId = bestId;
            _logger.Debug("{Name} acquired target {TargetId} at {Distance:F1}m", npc.Entity, bestId, bestDistance);
        }
    }

    private bool HasLineOfSight(CharacterEntity source, CharacterEntity target)
    {
        var physics = _shard.Physics;
        if (physics == null)
        {
            // No collision data loaded, so nothing can occlude. Treat everything as visible.
            return true;
        }

        var from = source.Position + new Vector3(0f, 0f, _eyeHeight);
        var to = target.Position + new Vector3(0f, 0f, _eyeHeight);
        var hit = physics.SegmentRayCast(from, to, source.EntityId);

        return !hit.Hit || hit.HitEntityId == target.EntityId;
    }

    private void ResolveAttack(NpcBrain npc, CharacterEntity target)
    {
        int damage = _rules.AttackDamage;
        _shard.Damage?.ApplyDamage(target, damage, npc.Entity);
        _feedback?.OnAttack(npc.Entity, target, damage);
    }

    private void ApplyDecision(NpcBrain npc, in AiDecision decision, ulong elapsedMs, CharacterEntity target)
    {
        var entity = npc.Entity;

        Vector3? goal = decision.Movement switch
        {
            AiMovementIntent.TowardTarget => new Vector3?(target?.Position ?? npc.Home),
            AiMovementIntent.TowardHome => new Vector3?(npc.Home),
            _ => null,
        };

        bool moved = false;
        if (goal.HasValue)
        {
            moved = MoveToward(npc, goal.Value, decision.State, elapsedMs);
        }

        if (decision.FaceTarget && target != null)
        {
            var facing = target.Position - entity.Position;
            if (facing.LengthSquared() > 0.0001f)
            {
                entity.SetOrientation(AiVectors.OrientationFacing(facing));

                // Only update the aim when the horizontal projection is
                // non-degenerate.  When the target is directly above or
                // below the NPC the XY vector has near-zero length and
                // Normalize would produce NaN, which later crashes the
                // pose serializer with ArithmeticException.
                var aimFlat = new Vector3(facing.X, facing.Y, 0f);
                if (aimFlat.LengthSquared() > 0.0001f)
                {
                    entity.AimDirection = Vector3.Normalize(aimFlat);
                }
            }
        }

        var movementState = moved ? _movementStateRunning : _movementStateIdle;
        entity.MovementState = movementState;
        BroadcastPose(entity, movementState);
    }

    private bool MoveToward(NpcBrain npc, Vector3 goal, AiBrainState state, ulong elapsedMs)
    {
        var entity = npc.Entity;

        var delta = goal - entity.Position;
        delta.Z = 0f;

        float horizontal = delta.Length();
        if (horizontal < 0.05f)
        {
            return false;
        }

        var direction = delta / horizontal;
        float speed = state == AiBrainState.Attack ? npc.MoveSpeed : npc.ChaseSpeed;
        float step = speed * (elapsedMs / 1000f);
        if (step > horizontal)
        {
            step = horizontal;
        }

        var candidate = entity.Position + (direction * step);
        if (IsBlocked(entity.Position, candidate, entity.EntityId))
        {
            return false;
        }

        candidate = ProbeGround(entity, candidate);

        entity.SetPosition(candidate);
        entity.SetOrientation(AiVectors.OrientationFacing(direction));
        entity.AimDirection = direction;
        _shard.Physics?.UpdateEntity(entity);

        return true;
    }

    private bool IsBlocked(Vector3 from, Vector3 to, ulong selfEntityId)
    {
        var physics = _shard.Physics;
        if (physics == null)
        {
            return false;
        }

        // Raise the ray to torso height so it does not graze the ground the NPC is
        // standing on (which would read as an obstacle after ground snapping).
        var raised = new Vector3(0f, 0f, _wallCheckHeight);
        var hit = physics.SegmentRayCast(from + raised, to + raised, selfEntityId);
        if (!hit.Hit)
        {
            return false;
        }

        // A graze right at the destination should not stop the NPC dead in its tracks.
        return hit.T < (Vector3.Distance(from, to) - 0.05f);
    }

    private Vector3 ProbeGround(CharacterEntity entity, Vector3 candidate)
    {
        var physics = _shard.Physics;
        if (!_rules.SnapToGround || physics == null)
        {
            return candidate;
        }

        var from = new Vector3(candidate.X, candidate.Y, candidate.Z + _groundProbeUp);
        var to = new Vector3(candidate.X, candidate.Y, candidate.Z - _groundProbeDown);

        // Only static geometry counts as ground; another mob or player standing
        // nearby must not be mistaken for terrain.
        var hit = physics.SegmentRayCast(from, to, entity.EntityId, staticOnly: true);
        if (!hit.Hit)
        {
            return candidate;
        }

        return new Vector3(candidate.X, candidate.Y, hit.HitPosition.Z + _rules.GroundOffset);
    }

    private void BroadcastPose(CharacterEntity entity, short movementState)
    {
        var pose = new AeroMessages.GSS.Character.Event.CurrentPoseUpdate
        {
            Data = new AeroMessages.GSS.CurrentPoseUpdateData
            {
                Flags = 0x00,
                ShortTime = _shard.CurrentShortTime,
                UnkAlwaysPresent = 0x79,
                MovementState = (ushort)movementState,
                Position = entity.Position,
                Rotation = entity.Orientation,
                Aim = entity.AimDirection,
            },
        };

        // Same delivery path MovementRelay uses for player movement: every playing client,
        // regardless of scope. Narrowing this to EntityMan.HasScopedInEntity would cut the
        // packet count on a busy shard, but a player whose scope-in is still queued would see
        // a frozen NPC, so stay on the path that is known to work.
        foreach (var client in _shard.Clients.Values)
        {
            if (client.Status.Equals(IPlayer.PlayerStatus.Playing) &&
                client.NetChannels.TryGetValue(ChannelType.UnreliableGss, out var channel))
            {
                channel.SendMessage(pose, entity.EntityId);
            }
        }
    }

    private sealed class NpcBrain
    {
        public ulong EntityId;
        public CharacterEntity Entity;
        public AiBrain Brain;
        public ulong TargetId;
        public Vector3 Home;
        public float MoveSpeed;
        public float ChaseSpeed;
    }
}
