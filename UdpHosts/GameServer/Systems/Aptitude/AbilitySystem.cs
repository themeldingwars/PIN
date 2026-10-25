using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using AeroMessages.GSS.Character.Command;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.StaticDB;
using Serilog;

namespace GameServer.Systems.Aptitude;

public class AbilitySystem
{
    private static readonly ILogger _logger = Log.ForContext<AbilitySystem>();
    private readonly Shard _shard;
    private readonly ulong _updateIntervalMs = 20;
    private readonly Dictionary<ulong, VehicleCalldownRequest> _playerVehicleCalldownRequests;
    private readonly Dictionary<ulong, DeployableCalldownRequest> _playerDeployableCalldownRequests;
    private readonly Dictionary<ulong, ResourceNodeBeaconCalldownRequest> _playerThumperCalldownRequests;
    private ulong _lastUpdate;

    public AbilitySystem(Shard shard)
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

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        if (currentTime > _lastUpdate + _updateIntervalMs)
        {
            _lastUpdate = currentTime;
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
            if (activeEffect?.Effect.DurationChain != null
                && currentTime > activeEffect.LastUpdateTime + activeEffect.Effect.UpdateFrequency)
            {
                activeEffect.Context.ExecutionHint = ExecutionHint.DurationEffect;
                activeEffect.Context.CurrentTime = _shard.CurrentTime;
                var durationResult = new CommandResult { Success = true };
                activeEffect.Effect.DurationChain.Execute(activeEffect.Context, ref durationResult);
                activeEffect.LastUpdateTime = currentTime;

                if (durationResult.Success)
                {
                    if (activeEffect.Effect.UpdateChain != null)
                    {
                        activeEffect.Context.ExecutionHint = ExecutionHint.UpdateEffect;
                        var updateResult = new CommandResult { Success = true };
                        activeEffect.Effect.UpdateChain.Execute(activeEffect.Context, ref updateResult);
                    }
                }
                else
                {
                    DoRemoveEffect(activeEffect);
                }
            }
        }
    }

    public void DoApplyEffect(uint effectId, IAptitudeTarget target, Context context)
    {
        if (effectId == 0)
        {
            return;
        }

        var applyContext = Context.CopyContext(context);
        applyContext.Self = target;
        applyContext.InitTime = context.CurrentTime;
        applyContext.CurrentTime = context.CurrentTime;
        applyContext.ExecutionHint = ExecutionHint.ApplyEffect;

        if (applyContext.ScaleMultiplier == 0f)
        {
            applyContext.ScaleMultiplier = 1.0f;
        }

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
            _logger.Debug(
                "Effect {EffectId} on {Target} is already at its {MaxStacks} stacks, so nothing of this application runs and the modifier already on the target is left alone",
                effectId,
                target,
                effect.MaxStackCount);
            return;
        }

        var wasAlreadyActive = !ReferenceEquals(effectState.Context, applyContext);

        var applyResult = new CommandResult { Success = true };
        effect.ApplyChain?.Execute(applyContext, ref applyResult);

        using var logContext = Serilog.Context.LogContext.PushProperty("ExecutionId", applyContext.ExecutionId);
        foreach (var pair in applyContext.Actives)
        {
            ICommand activeCommand = pair.Key;
            activeCommand.OnApply(applyContext, pair.Value);
        }

        if (wasAlreadyActive)
        {
            _logger.Debug(
                "Effect {EffectId} on {Target} was already active and is now {Stacks} stacks; the {StatCount} stat modifiers of this application are not tracked: {Stats}",
                effectId,
                target,
                effectState.Stacks,
                applyContext.StatChangelist.Count,
                string.Join(", ", applyContext.StatChangelist.Keys));
        }
        else if (applyContext.StatChangelist.Count > 0 && effect.DurationChain == null)
        {
            _logger.Debug(
                "Effect {EffectId} on {Target} holds stat modifiers {Stats} and has no duration chain, so nothing expires it",
                effectId,
                target,
                string.Join(", ", applyContext.StatChangelist.Keys));
        }
    }

    public void DoRemoveEffect(EffectState activeEffect)
    {
        activeEffect.Context.ExecutionHint = ExecutionHint.RemoveEffect;
        activeEffect.Context.CurrentTime = _shard.CurrentTime;
        activeEffect.Context.Self.ClearEffect(activeEffect);
        var removeResult = new CommandResult { Success = true };
        activeEffect.Effect.RemoveChain?.Execute(activeEffect.Context, ref removeResult);

        using var logContext = Serilog.Context.LogContext.PushProperty("ExecutionId", activeEffect.Context.ExecutionId);
        foreach (var pair in activeEffect.Context.Actives)
        {
            ICommand activeCommand = pair.Key;
            activeCommand.OnRemove(activeEffect.Context, pair.Value);
        }
    }

    public void DoRemoveEffect(IAptitudeTarget entity, uint effectId)
    {
        var activeEffects = entity.GetActiveEffects();
        foreach (var activeEffect in activeEffects)
        {
            if (activeEffect?.Effect.Id != null)
            {
                if (activeEffect.Effect.Id == effectId)
                {
                    DoRemoveEffect(activeEffect);
                    break;
                }
            }
        }
    }

    public void RemoveAllActiveEffects(IAptitudeTarget target)
    {
        if (target is CharacterEntity { InteractionTarget: not null, InteractionExecutionId: not null } character)
        {
            HandleDeactivateAbility(character, character.InteractionExecutionId.Value);
        }

        var removed = 0;

        // GetActiveEffects hands out a copy of the slots, so effects can be removed while walking the list.
        foreach (var effectState in target.GetActiveEffects())
        {
            if (effectState != null)
            {
                DoRemoveEffect(effectState);
                removed++;
            }
        }

        _logger.Debug("RemoveAllActiveEffects: removed {RemovedCount} active effects from {Target}", removed, target);
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
        _logger.Information("HandleLocalProximityAbilitySuccess Source {source}, Command {commandId}, Time {time}, TargetsCount {targetsCount}", source, commandId, time, targets.Count);

        var commandDef = SDBInterface.GetRegisterClientProximityCommandDef(commandId);

        IAptitudeTarget triggerer = targets.OfType<CharacterEntity>().FirstOrDefault();

        if (commandDef.AbilityId != 0)
        {
            HandleActivateAbility(shard, source, commandDef.AbilityId, time, targets, execId, self: triggerer);
        }

        if (commandDef.Chain != 0)
        {
            var chain = Factory.LoadChain(commandDef.Chain);
            var localProxResult = new CommandResult { Success = true };
            var context = new Context(shard, source, triggerer)
            {
                ExecutionId = execId,
                ChainId = commandDef.Chain,
                Targets = targets,
                InitTime = time,
                CurrentTime = time,
                ExecutionHint = ExecutionHint.Proximity
            };

            context.SnapshotInitPose();
            chain.Execute(context, ref localProxResult);
        }
    }

    public void HandleActivateAbility(IShard shard, IAptitudeTarget initiator, uint abilityId, uint activationTime, AptitudeTargets targets, Guid? executionId = null, Vector3? initPosition = null, IAptitudeTarget self = null)
    {
        var execId = executionId ?? Guid.NewGuid();
        using var logContext = Serilog.Context.LogContext.PushProperty("ExecutionId", execId);
        var abilityData = SDBInterface.GetAbilityData(abilityId);
        if (abilityData == null)
        {
            _logger.Warning("HandleActivateAbility: Ability {AbilityId} has no AbilityData, skipping", abilityId);
            return;
        }

        var chainId = abilityData.Chain;
        if (chainId == 0)
        {
            return;
        }

        _logger.Information("HandleActivateAbility: Ability {AbilityId} starting Chain {ChainId}", abilityId, chainId);

        var context = new Context(shard, initiator, self)
        {
            ExecutionId = execId,
            ChainId = chainId,
            AbilityId = abilityId,
            Targets = targets,
            CurrentTime = activationTime,
            ExecutionHint = ExecutionHint.Ability
        };
        if (initPosition.HasValue)
        {
            context.InitPosition = initPosition.Value;
        }

        if (!ReferenceEquals(context.Self, initiator))
        {
            context.SnapshotInitPose();
        }

        var chain = Factory.LoadChain(chainId);
        var abilityResult = new CommandResult { Success = true };
        chain.Execute(context, ref abilityResult);
    }

    public void HandleActivateAbility(IShard shard, IAptitudeTarget initiator, uint abilityId)
    {
        HandleActivateAbility(shard, initiator, abilityId, _shard.CurrentTime, new AptitudeTargets());
    }

    public void HandleTargetAbility()
    {
        throw new NotImplementedException();
    }

    public void HandleDeactivateAbility(IAptitudeTarget target, Guid executionId)
    {
        // Tear down a held interaction when its activation input is released. Tap / buff abilities are left
        // alone: the effects they applied keep their own duration and must not be stripped on key release.
        if (target is CharacterEntity { InteractionTarget: not null } character && character.InteractionExecutionId == executionId)
        {
            _logger.Information("HandleDeactivateAbility: ending interaction execution {ExecutionId} for {Target}", executionId, target);

            // Effects applied during the interaction chain share its ExecutionId (ImpactApplyEffectCommand /
            // Context.CopyContext preserve it), so removing them by id cleanly tears down the interaction.
            foreach (var effectState in target.GetActiveEffects())
            {
                if (effectState?.Context != null && effectState.Context.ExecutionId == executionId)
                {
                    DoRemoveEffect(effectState);
                }
            }

            character.SetInteractionTarget(null);
        }
    }

    public void HandleActivateConsumable()
    {
        throw new NotImplementedException();
    }
}