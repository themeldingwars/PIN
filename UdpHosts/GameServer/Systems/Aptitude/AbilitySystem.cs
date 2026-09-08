using System;
using System.Collections.Generic;
using System.Threading;
using AeroMessages.GSS.Character.Command;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.Extensions;
using GameServer.StaticDB;
using Serilog;

namespace GameServer.Systems.Aptitude;

public class AbilitySystem
{
    private static readonly ILogger _logger = Log.ForContext<AbilitySystem>();
    private readonly IShard _shard;
    private readonly ulong _updateIntervalMs = 20;
    private readonly Dictionary<ulong, VehicleCalldownRequest> _playerVehicleCalldownRequests;
    private readonly Dictionary<ulong, DeployableCalldownRequest> _playerDeployableCalldownRequests;
    private readonly Dictionary<ulong, ResourceNodeBeaconCalldownRequest> _playerThumperCalldownRequests;
    private readonly Dictionary<ulong, AbilityState> _entityAbilityStates = [];

    /// <summary>
    /// Cooldown category per ability id, learned from the activation command of
    /// the ability chain the first time it runs. <c>dbitems::AbilityModule.UiCategory</c>
    /// is a UI grouping and is NOT the aptitude cooldown category, so it cannot
    /// be used to look up category cooldowns.
    /// </summary>
    private readonly Dictionary<uint, uint> _abilityCooldownCategories = [];

    /// <summary>
    /// Shard time of the last accepted client proximity activation, per (entity, proximity command). Used to
    /// hold activations to the retry interval of <c>aptfs::RegisterClientProximityCommandDef</c>.
    /// </summary>
    private readonly Dictionary<(ulong EntityId, uint CommandId), uint> _proximityActivations = [];

    /// <summary>
    /// The effects the last accepted activation of a client proximity command applied, per (entity, command).
    /// While any of them is still on its target the command must not run again: the client keeps sending
    /// the success message for as long as the player stands on the trigger (a glider pad), and re-running the
    /// chain re-applies the same effects, spawns another projectile and rewrites the status effect fields of
    /// the pad and of the player, which is what floods the connection of everyone in range.
    /// </summary>
    private readonly Dictionary<(ulong EntityId, uint CommandId), List<AppliedEffectRecord>> _proximityAppliedEffects = [];

    /// <summary>
    /// Movement-state bindings registered by <c>RegisterMovementEffectCommand</c>, per character. The ability
    /// system re-evaluates them on its tick: while the character is in the bound movement state the status
    /// effect is applied, and the moment it leaves that state (or the carrying effect ends) it is removed.
    /// </summary>
    private readonly Dictionary<ulong, List<MovementEffectRegistration>> _movementEffectRegistrations = [];

    private uint _proximityActivationsPruneAt;

    private ulong _lastUpdate;

    public AbilitySystem(IShard shard)
    {
        _shard = shard;
        Factory = new Factory(shard);
        _playerVehicleCalldownRequests = [];
        _playerDeployableCalldownRequests = [];
        _playerThumperCalldownRequests = [];
    }

    public Factory Factory { get; }

    public static float RegistryOp(float first, float second, Operand op)
    {
        if (float.IsNaN(first))
        {
            return second;
        }

        switch (op)
        {
            case Operand.ASSIGN:
                return second;
            case Operand.ADD:
            case Operand.ADD_ALT:
                return second + first;
            case Operand.MULTIPLY:
            case Operand.MULTIPLY_ALT:
                return second * first;
            case Operand.EXPONENTIATE:
                _logger.Debug("Uncertain RegistryOp {op}. {second} ^ {first} = {result}", op, second, first, (float)Math.Pow(second, first));
                return (float)Math.Pow(second, first);
            case Operand.SUBTRACT:
                return second - first;
            case Operand.DIVIDE:
                return second / first;
            case Operand.MINIMUM:
                return (first <= second) ? first : second;
            case Operand.MAXIMUM:
                return (first >= second) ? first : second;
            default:
                _logger.Warning("Unknown RegistryOp {op}", op);
                return second;
        }
    }

    /// <summary>Returns the (created on first use) cooldown/energy state of an aptitude entity.</summary>
    public AbilityState GetOrAddState(IAptitudeTarget entity)
    {
        if (!_entityAbilityStates.TryGetValue(entity.EntityId, out var state))
        {
            state = new AbilityState
            {
                LastEnergyUpdateTime = _shard.CurrentTime,
            };

            // Mirror the energy pool configuration that was sent to the client
            // (CharacterEntity.EnergyParams) so server-side energy requirements
            // and costs use the same scale and recharge rhythm as the client
            // simulated pool.
            if (entity is CharacterEntity character && character.EnergyParams.Max > 0f)
            {
                state.MaxEnergy = character.EnergyParams.Max;
                state.Energy = character.EnergyParams.Max;
                state.EnergyRegenPerSecond = character.EnergyParams.Recharge;
                state.EnergyRegenDelayMs = character.EnergyParams.Delay;
                state.LastEnergySpendTime = _shard.CurrentTime;
            }

            _entityAbilityStates[entity.EntityId] = state;
        }

        else if (entity is CharacterEntity existingCharacter && existingCharacter.EnergyParams.Max > 0f)
        {
            // A state can be created by cooldown handling before the character's
            // spawn data has finished populating EnergyParams. Do not leave that
            // state permanently on the 100-energy fallback configuration.
            state.MaxEnergy = existingCharacter.EnergyParams.Max;
            state.EnergyRegenPerSecond = existingCharacter.EnergyParams.Recharge;
            state.EnergyRegenDelayMs = existingCharacter.EnergyParams.Delay;
            if (state.Energy > state.MaxEnergy)
            {
                state.Energy = state.MaxEnergy;
            }
        }

        return state;
    }

    /// <summary>
    /// Records the aptitude cooldown category an ability belongs to, as read
    /// from its activation command.
    /// </summary>
    public void RegisterAbilityCategory(uint abilityId, uint category)
    {
        if (abilityId == 0 || category == 0)
        {
            return;
        }

        _abilityCooldownCategories[abilityId] = category;
    }

    /// <summary>Returns the known aptitude cooldown category of an ability, or <c>0</c>.</summary>
    public uint GetAbilityCategory(uint abilityId)
    {
        return _abilityCooldownCategories.GetValueOrDefault(abilityId);
    }

    public bool TryGetState(IAptitudeTarget entity, out AbilityState state)
    {
        return _entityAbilityStates.TryGetValue(entity.EntityId, out state);
    }

    public bool TryGetState(ulong entityId, out AbilityState state)
    {
        return _entityAbilityStates.TryGetValue(entityId, out state);
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        if (currentTime > _lastUpdate + _updateIntervalMs)
        {
            _lastUpdate = currentTime;
            uint time = unchecked((uint)currentTime);

            // Drop states whose entity left the shard, prune expired cooldowns and
            // advance passive energy regen for the rest.
            List<ulong> stale = null;
            foreach (var pair in _entityAbilityStates)
            {
                if (!_shard.Entities.ContainsKey(pair.Key))
                {
                    (stale ??= []).Add(pair.Key);
                    continue;
                }

                pair.Value.Prune(time);
                pair.Value.UpdateEnergy(time);
            }

            if (stale != null)
            {
                foreach (var entityId in stale)
                {
                    _entityAbilityStates.Remove(entityId);
                }

                stale = null;
            }

            // A character that left the shard cannot leave its movement-state bindings behind either: the
            // bindings are keyed by entity id and would otherwise leak (and could apply effects to an entity
            // id the shard later reuses).
            foreach (var pair in _movementEffectRegistrations)
            {
                if (!_shard.Entities.ContainsKey(pair.Key))
                {
                    (stale ??= []).Add(pair.Key);
                }
            }

            if (stale != null)
            {
                foreach (var entityId in stale)
                {
                    _movementEffectRegistrations.Remove(entityId);
                }
            }

            foreach (var entity in _shard.Entities.Values)
            {
                if (entity is IAptitudeTarget target)
                {
                    ProcessTarget(target, currentTime);
                }
            }
        }
    }

    public void ProcessTarget(IAptitudeTarget entity, ulong currentTime)
    {
        var activeEffects = entity.GetActiveEffects();
        foreach (var activeEffect in activeEffects)
        {
            // Removing an earlier effect can destroy this entity itself: a
            // RemoveEffect chain may end in DestroyAbilityObjectCommand, which
            // removes the entity from the shard. The snapshot above still
            // lists the entity's remaining effects, but they belong to an
            // entity that no longer exists, so stop processing it instead of
            // flushing network changes for the removed entity.
            if (!_shard.Entities.ContainsKey(entity.EntityId))
            {
                break;
            }

            if (activeEffect?.Effect.DurationChain != null
                && currentTime > activeEffect.LastUpdateTime + activeEffect.Effect.UpdateFrequency)
            {
                activeEffect.Context.ExecutionHint = ExecutionHint.DurationEffect;
                bool durationResult = activeEffect.Effect.DurationChain.Execute(activeEffect.Context);
                activeEffect.LastUpdateTime = currentTime;

                if (durationResult)
                {
                    if (activeEffect.Effect.UpdateChain != null)
                    {
                        activeEffect.Context.ExecutionHint = ExecutionHint.UpdateEffect;
                        activeEffect.Effect.UpdateChain.Execute(activeEffect.Context);
                    }
                }
                else
                {
                    DoRemoveEffect(activeEffect);
                }
            }
        }

        ReevaluateMovementEffects(entity, currentTime);
    }

    /// <summary>
    /// Registers a movement-state binding for a character: while the character is in the given movement state
    /// the status effect is applied, and it is removed when the character leaves that state. Returns the
    /// registration, which <c>RegisterMovementEffectCommand</c> hands back to
    /// <see cref="UnregisterMovementEffect" /> when the carrying effect ends.
    /// </summary>
    public MovementEffectRegistration RegisterMovementEffect(CharacterEntity character, uint statusfxId, byte movestateIndex, bool requireSprint)
    {
        var registration = new MovementEffectRegistration
        {
            CharacterEntityId = character.EntityId,
            StatusfxId = statusfxId,
            MovestateIndex = movestateIndex,
            RequireSprint = requireSprint,
        };

        if (!_movementEffectRegistrations.TryGetValue(character.EntityId, out var registrations))
        {
            registrations = [];
            _movementEffectRegistrations[character.EntityId] = registrations;
        }

        registrations.Add(registration);

        return registration;
    }

    /// <summary>
    /// Removes a movement-state binding and, if the bound effect is still on the character, takes it off again.
    /// </summary>
    public void UnregisterMovementEffect(MovementEffectRegistration registration)
    {
        if (registration == null)
        {
            return;
        }

        if (_movementEffectRegistrations.TryGetValue(registration.CharacterEntityId, out var registrations))
        {
            registrations.Remove(registration);

            if (registrations.Count == 0)
            {
                _movementEffectRegistrations.Remove(registration.CharacterEntityId);
            }
        }
    }

    private void ReevaluateMovementEffects(IAptitudeTarget entity, ulong currentTime)
    {
        if (entity is not CharacterEntity character)
        {
            return;
        }

        // A remove chain that destroys the entity mid-tick leaves the object behind but out of the shard;
        // do not apply or remove effects for an entity the shard no longer simulates.
        if (!_shard.Entities.ContainsKey(character.EntityId))
        {
            return;
        }

        if (!_movementEffectRegistrations.TryGetValue(character.EntityId, out var registrations))
        {
            return;
        }

        var movestateIndex = (byte)((int)character.MovementStateContainer.Movestate >> 4);
        var sprinting = character.MovementStateContainer.Sprint;
        var time = unchecked((uint)currentTime);

        // Snapshot: removing an effect below can run a remove chain that unregisters a binding of the same
        // character, which would mutate the very list being iterated.
        foreach (var registration in [.. registrations])
        {
            var inState = registration.MovestateIndex == movestateIndex
                && (!registration.RequireSprint || sprinting);

            var applied = HasEffect(character, registration.StatusfxId);

            if (inState && !applied)
            {
                DoApplyEffect(registration.StatusfxId, character, new Context(_shard, character) { InitTime = time });
            }
            else if (!inState && applied)
            {
                DoRemoveEffect(character, registration.StatusfxId);
            }
        }
    }

    private static bool HasEffect(IAptitudeTarget target, uint effectId)
    {
        foreach (var activeEffect in target.GetActiveEffects())
        {
            if (activeEffect?.Effect.Id == effectId)
            {
                return true;
            }
        }

        return false;
    }

    public bool DoApplyEffect(uint effectId, IAptitudeTarget target, Context context)
    {
        if (effectId == 0)
        {
            return true;
        }

        var applyContext = Context.CopyContext(context);
        applyContext.Self = target;
        applyContext.ExecutionHint = ExecutionHint.ApplyEffect;

        var effect = Factory.LoadEffect(effectId);

        // TODO: Decouple effect storage from fields so that hidden effects can be added without using a network field
        /*
        if (effect.Data.Hidden == 0)
        {

        }
        */
        var effectState = target.AddEffect(effect, applyContext);

        if (effectState.MaxStacksExceeded)
        {
            return true;
        }

        context.AppliedEffects?.Add(new AppliedEffectRecord(target, effectState));

        bool applyResult = effect.ApplyChain?.Execute(applyContext) ?? true;
        if (!applyResult)
        {
            // A failed requirement inside an effect apply chain is part of the
            // enclosing activation. Do not leave the newly-added effect behind
            // when that chain rejects the activation.
            target.ClearEffect(effectState);
            return false;
        }

        using var logContext = Serilog.Context.LogContext.PushProperty("ExecutionId", applyContext.ExecutionId);
        foreach (var pair in applyContext.Actives)
        {
            ICommand activeCommand = pair.Key;
            activeCommand.OnApply(applyContext, pair.Value);
        }

        return true;
    }

    public bool DoRemoveEffect(EffectState activeEffect)
    {
        if (activeEffect == null)
        {
            return true;
        }

        activeEffect.Context.ExecutionHint = ExecutionHint.RemoveEffect;
        activeEffect.Context.Self.ClearEffect(activeEffect);
        bool removeResult = activeEffect.Effect.RemoveChain?.Execute(activeEffect.Context) ?? true;
        if (!removeResult)
        {
            return false;
        }

        using var logContext = Serilog.Context.LogContext.PushProperty("ExecutionId", activeEffect.Context.ExecutionId);
        foreach (var pair in activeEffect.Context.Actives)
        {
            ICommand activeCommand = pair.Key;
            activeCommand.OnRemove(activeEffect.Context, pair.Value);
        }

        return true;
    }

    public bool DoRemoveEffect(IAptitudeTarget entity, uint effectId)
    {
        var activeEffects = entity.GetActiveEffects();
        foreach (var activeEffect in activeEffects)
        {
            if (activeEffect?.Effect.Id == effectId)
            {
                return DoRemoveEffect(activeEffect);
            }
        }

        return true;
    }

    public VehicleCalldownRequest TryConsumeVehicleCalldownRequest(ulong entityId)
    {
        return _playerVehicleCalldownRequests.Remove(entityId, out var result) ? result : null;
    }

    public DeployableCalldownRequest TryConsumeDeployableCalldownRequest(ulong entityId)
    {
        return _playerDeployableCalldownRequests.Remove(entityId, out var result) ? result : null;
    }

    public ResourceNodeBeaconCalldownRequest TryConsumeResourceNodeBeaconCalldownRequest(ulong entityId)
    {
        return _playerThumperCalldownRequests.Remove(entityId, out var result) ? result : null;
    }

    public void HandleVehicleCalldownRequest(ulong entityId, VehicleCalldownRequest request)
    {
        if (_playerVehicleCalldownRequests.ContainsKey(entityId))
        {
            _logger.Information("Discarded an unconsumed vehicle calldown request for {entityId}", entityId);
            _playerVehicleCalldownRequests.Remove(entityId);
        }

        _playerVehicleCalldownRequests.Add(entityId, request);
    }

    public void HandleDeployableCalldownRequest(ulong entityId, DeployableCalldownRequest request)
    {
        if (_playerDeployableCalldownRequests.ContainsKey(entityId))
        {
            _logger.Information("Discarded an unconsumed deployable calldown request for {entityId}", entityId);
            _playerDeployableCalldownRequests.Remove(entityId);
        }

        _playerDeployableCalldownRequests.Add(entityId, request);
    }

    public void HandleResourceNodeBeaconCalldownRequest(ulong entityId, ResourceNodeBeaconCalldownRequest request)
    {
        if (_playerThumperCalldownRequests.ContainsKey(entityId))
        {
            _logger.Information("Discarded an unconsumed thumper calldown request for {entityId}", entityId);
            _playerThumperCalldownRequests.Remove(entityId);
        }

        _playerThumperCalldownRequests.Add(entityId, request);
    }

    public void HandleLocalProximityAbilitySuccess(IShard shard, IAptitudeTarget source, uint commandId, uint time, AptitudeTargets targets, Guid? executionId = null)
    {
        var execId = executionId ?? Guid.NewGuid();
        using var logContext = Serilog.Context.LogContext.PushProperty("ExecutionId", execId);
        // A client that stays in range of a proximity trigger keeps sending this success several times a
        // second, so the arrival on its own is only worth a Verbose line. The Debug line below the retry gate
        // marks the activations that actually run something.
        _logger.Verbose("HandleLocalProximityAbilitySuccess Source {source}, Command {commandId}, Time {time}, TargetsCount {targetsCount}", source, commandId, time, targets.Count);

        var commandDef = SDBInterface.GetRegisterClientProximityCommandDef(commandId);

        if (commandDef == null)
        {
            // The client is allowed to name a proximity command this shard does not know (an item or battleframe
            // whose aptitude data is not in the database we loaded). This used to throw a NullReferenceException
            // straight out of the network tick.
            if (OnceLog.ShouldLog(("unknown-proximity-command", commandId)))
            {
                _logger.Warning("HandleLocalProximityAbilitySuccess: proximity command {CommandId} is not in aptfs::RegisterClientProximityCommandDef, ignoring", commandId);
            }

            return;
        }

        if (IsProximityEffectStillActive(source, commandId))
        {
            // A previous activation is still applied. Nothing would change by running the chain again, so skip
            // it entirely instead of removing and re-adding the very same effects dozens of times a second.
            _logger.Verbose("HandleLocalProximityAbilitySuccess: {Source} still carries the effects of proximity command {CommandId}, ignoring", source, commandId);

            return;
        }

        if (!AllowProximityActivation(source, commandId, time, commandDef.RetryInterval))
        {
            // The client re-triggers a proximity ability for as long as the player stays in range, on a retry
            // interval of its own. Everything the ability does is applied through effects, so re-running the whole
            // chain several times per second only multiplies the work: with the effect re-applied and lost again
            // on every run (which is what the not implemented commands used to cause), the status effect fields of
            // the pad and of the player were rewritten and flushed to every client in range dozens of times a
            // second, which is what stalled the connection of the player standing on the panel.
            _logger.Verbose("HandleLocalProximityAbilitySuccess: {Source} is still inside the retry interval ({RetryInterval} ms) of proximity command {CommandId}, ignoring", source, commandDef.RetryInterval, commandId);

            return;
        }

        _logger.Debug(
            "HandleLocalProximityAbilitySuccess: running command {CommandId} for {Source} against {TargetsCount} targets",
            commandId,
            source,
            targets.Count);

        List<AppliedEffectRecord> applied = [];

        if (commandDef.AbilityId != 0)
        {
            HandleActivateAbility(shard, source, commandDef.AbilityId, time, targets, execId, appliedEffects: applied);
        }

        if (commandDef.Chain != 0)
        {
            var chain = Factory.LoadChain(commandDef.Chain);
            chain.Execute(new Context(shard, source)
            {
                ExecutionId = execId,
                ChainId = commandDef.Chain,
                Targets = targets,
                InitTime = time,
                ExecutionHint = ExecutionHint.Proximity,
                AppliedEffects = applied
            });
        }

        if (source != null)
        {
            var key = (source.EntityId, commandId);

            if (applied.Count > 0)
            {
                _proximityAppliedEffects[key] = applied;
            }
            else
            {
                _proximityAppliedEffects.Remove(key);
            }
        }
    }

    /// <summary>
    /// True while at least one effect applied by the last accepted activation of this proximity command is
    /// still on its target, i.e. while re-running the command would only re-apply what is already there.
    /// </summary>
    private bool IsProximityEffectStillActive(IAptitudeTarget source, uint commandId)
    {
        if (source == null)
        {
            return false;
        }

        var key = (source.EntityId, commandId);

        if (!_proximityAppliedEffects.TryGetValue(key, out var applied))
        {
            return false;
        }

        foreach (var record in applied)
        {
            if (record.IsStillActive())
            {
                return true;
            }
        }

        _proximityAppliedEffects.Remove(key);

        return false;
    }

    /// <summary>
    /// Decides whether an entity may run this proximity command again, honouring the retry interval the
    /// registration gives it.
    /// </summary>
    private bool AllowProximityActivation(IAptitudeTarget source, uint commandId, uint time, uint retryIntervalMs)
    {
        if (source == null)
        {
            return true;
        }

        var key = (source.EntityId, commandId);

        if (retryIntervalMs > 0
            && _proximityActivations.TryGetValue(key, out uint lastActivation)
            && unchecked(time - lastActivation) < retryIntervalMs)
        {
            return false;
        }

        _proximityActivations[key] = time;

        if (unchecked((int)(time - _proximityActivationsPruneAt)) > 0)
        {
            PruneProximityActivations(time);
        }

        return true;
    }

    /// <summary>
    /// Forget the activations of entities that are gone, so the bookkeeping of a long running shard does not
    /// grow without a bound. Runs at most once every thirty seconds of shard time.
    /// </summary>
    private void PruneProximityActivations(uint time)
    {
        _proximityActivationsPruneAt = time + 30_000;

        if (_proximityActivations.Count <= 1024)
        {
            return;
        }

        List<(ulong EntityId, uint CommandId)> stale = [];

        foreach (var key in _proximityActivations.Keys)
        {
            if (!_shard.Entities.ContainsKey(key.EntityId))
            {
                stale.Add(key);
            }
        }

        foreach (var key in stale)
        {
            _proximityActivations.Remove(key);
            _proximityAppliedEffects.Remove(key);
        }
    }

    /// <summary>
    /// Executes the chain of an activated ability and returns whether the whole
    /// chain succeeded (requirements like cooldowns or energy can fail it).
    /// </summary>
    public bool HandleActivateAbility(IShard shard, IAptitudeTarget initiator, uint abilityId, uint activationTime, AptitudeTargets targets, Guid? executionId = null, uint abilityModuleId = 0, List<AppliedEffectRecord> appliedEffects = null)
    {
        var execId = executionId ?? Guid.NewGuid();
        using var logContext = Serilog.Context.LogContext.PushProperty("ExecutionId", execId);
        var ability = SDBInterface.GetAbilityData(abilityId);
        if (ability == null)
        {
            _logger.Warning("HandleActivateAbility: Ability {AbilityId} was not found", abilityId);
            return false;
        }

        if (ability.Chain == 0)
        {
            _logger.Warning("HandleActivateAbility: Ability {AbilityId} has no chain, treating activation as a no-op", abilityId);
            return true;
        }

        var context = new Context(shard, initiator)
        {
            ExecutionId = execId,
            ChainId = ability.Chain,
            AbilityId = abilityId,
            AbilityModuleId = abilityModuleId,
            Targets = targets ?? new AptitudeTargets(),
            InitTime = activationTime,
            ExecutionHint = ExecutionHint.Ability,
            AppliedEffects = appliedEffects,
        };

        return ExecuteAbilityActivation(context, abilityId, ability.Chain, isRootActivation: true);
    }

    /// <summary>
    /// Executes an ability called from another aptitude chain. The called
    /// ability gets its own identity, while the caster, module, and pending
    /// cooldown list remain those of the root activation.
    /// </summary>
    internal bool HandleCalledAbility(Context parentContext, uint abilityId)
    {
        var ability = SDBInterface.GetAbilityData(abilityId);
        if (ability == null)
        {
            _logger.Warning("HandleCalledAbility: Ability {AbilityId} was not found", abilityId);
            return false;
        }

        if (ability.Chain == 0)
        {
            _logger.Warning("HandleCalledAbility: Ability {AbilityId} has no chain", abilityId);
            return true;
        }

        var context = Context.CopyContext(parentContext);
        // Call used to start the called ability with an empty target set. Keep
        // that targeting boundary while carrying the activation identity
        // across it.
        context.Targets = new AptitudeTargets();
        context.FormerTargets = new AptitudeTargets();
        context.TargetStack = new Stack<AptitudeTargets>();
        context.ChainId = ability.Chain;
        context.AbilityId = abilityId;
        context.ExecutionHint = ExecutionHint.Ability;
        return ExecuteAbilityActivation(context, abilityId, ability.Chain, isRootActivation: false);
    }

    private bool ExecuteAbilityActivation(Context context, uint abilityId, uint chainId, bool isRootActivation)
    {
        _logger.Information(
            "HandleActivateAbility: Ability {AbilityId} starting Chain {ChainId} (module {AbilityModuleId})",
            abilityId,
            chainId,
            context.AbilityModuleId);

        var chain = Factory.LoadChain(chainId);
        // Keep the resolved command list visible while debugging activations.
        // This distinguishes a missing command from a factory/SDB mapping
        // problem without requiring a debugger.
        foreach (var command in chain.Commands)
        {
            _logger.Information(
                "Ability {AbilityId} chain {ChainId} resolved command {CommandId} as {CommandType}",
                abilityId,
                chainId,
                command.Id,
                command.GetType().FullName);
        }

        bool success = chain.Execute(context);

        // The root activation owns cooldown commit: an ability that fails a
        // later requirement (for example a nested chain rejection) must not go
        // on cooldown. Called abilities queue into the shared pending list,
        // which the root commits when the whole activation succeeds.
        if (isRootActivation)
        {
            CommitActivationCooldowns(context, success);
        }

        return success;
    }

    public bool HandleActivateAbility(IShard shard, IAptitudeTarget initiator, uint abilityId)
    {
        return HandleActivateAbility(shard, initiator, abilityId, _shard.CurrentTime, new AptitudeTargets());
    }

    /// <summary>
    /// True when no still-running cooldown applies to the ability. Returns the
    /// time the ability will be usable again when it is cooling down.
    /// </summary>
    public bool IsAbilityReady(IAptitudeTarget entity, uint abilityId, uint category, uint time, out uint readyAgainTime)
    {
        readyAgainTime = time;
        if (!TryGetState(entity, out var state))
        {
            return true;
        }

        // Prefer the aptitude cooldown category learned from the ability's
        // activation command; the caller may only know the UI category.
        uint knownCategory = GetAbilityCategory(abilityId);
        if (knownCategory != 0)
        {
            category = knownCategory;
        }

        var blocking = state.GetActiveCooldown(abilityId, category, time);
        if (blocking == null)
        {
            return true;
        }

        readyAgainTime = blocking.ReadyAgainTime;
        return false;
    }

    public void HandleTargetAbility()
    {
        throw new NotImplementedException();
    }

    public void HandleDeactivateAbility()
    {
        throw new NotImplementedException();
    }

    public void HandleActivateConsumable()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Starts the cooldowns queued by the activation node once the chain has
    /// succeeded. Cooldowns are deliberately not started inside the activation
    /// command: an ability that fails a later requirement (for example not
    /// enough energy) must not go on cooldown.
    /// <para>
    /// The pending list is shared with copied contexts, so cooldowns queued by
    /// nested chains (branches, applied effects) are committed with the
    /// activation they belong to.
    /// </para>
    /// </summary>
    private void CommitActivationCooldowns(Context context, bool success)
    {
        if (!success || context.PendingCooldowns.Count == 0)
        {
            return;
        }

        uint time = _shard.CurrentTime;
        // Cooldowns belong to the activating caster, not to a target selected
        // by a nested targeting/effect command.
        var state = context.Abilities.GetOrAddState(context.ActivationInitiator);
        foreach (var request in context.PendingCooldowns)
        {
            var entry = state.StartCooldown(request.Kind, request.AbilityId, request.Category, request.DurationMs, time);
            if (entry != null)
            {
                _logger.Debug(
                    "Started {Kind} cooldown for ability {AbilityId} (category {Category}) for {Duration}ms",
                    request.Kind,
                    request.AbilityId,
                    request.Category,
                    entry.ReadyAgainTime - entry.ActivatedTime);
            }
        }
    }
}