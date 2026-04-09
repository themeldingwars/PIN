using System;
using System.Collections.Generic;
using System.Threading;
using AeroMessages.GSS.V66.Character.Command;
using GameServer.Data.SDB;
using GameServer.Enums;
using Serilog;

namespace GameServer.Aptitude;

public class AbilitySystem
{
    private static readonly ILogger _logger = Log.ForContext<AbilitySystem>();
    private Shard Shard;
    private Dictionary<ulong, VehicleCalldownRequest> PlayerVehicleCalldownRequests;
    private Dictionary<ulong, DeployableCalldownRequest> PlayerDeployableCalldownRequests;
    private Dictionary<ulong, ResourceNodeBeaconCalldownRequest> PlayerThumperCalldownRequests;

    private ulong LastUpdate;
    private ulong UpdateIntervalMs = 20;

    public AbilitySystem(Shard shard)
    {
        Shard = shard;
        Factory = new Factory(shard);
        PlayerVehicleCalldownRequests = new();
        PlayerDeployableCalldownRequests = new();
        PlayerThumperCalldownRequests = new();
    }
    
    public Factory Factory { get; }

    public static float RegistryOp(float first, float second, Operand op)
    {
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
                _logger.Debug("Uncertain RegistryOp {op}. {second} - {first} = {result}", op, second, first, second - first);
                return second - first;
            case Operand.DIVIDE:
                _logger.Debug("Uncertain RegistryOp {op}. {second} / {first} = {result}", op, second, first, second / first);
                return second / first;
            case Operand.MINIMUM:
                _logger.Debug("Uncertain RegistryOp {op}. Min({second}, {first}) = {result}", op, second, first, (first <= second) ? first : second);
                return (first <= second) ? first : second;
            case Operand.MAXIMUM:
                _logger.Debug("Uncertain RegistryOp {op}. Max({second}, {first}) = {result}", op, second, first, (first >= second) ? first : second);
                return (first >= second) ? first : second;
            default:
                _logger.Warning("Unknown RegistryOp {op}", op);
                return second;
        }
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        if (currentTime > LastUpdate + UpdateIntervalMs)
        {
            LastUpdate = currentTime;
            foreach (var entity in Shard.Entities.Values)
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
    }

    public void DoApplyEffect(uint effectId, IAptitudeTarget target, Context context)
    {
        if (effectId == 0)
        {
            return;
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
            return;
        }

        effect.ApplyChain?.Execute(applyContext);

        using var logContext = Serilog.Context.LogContext.PushProperty("ExecutionId", applyContext.ExecutionId);
        foreach (var pair in applyContext.Actives)
        {
            ICommand activeCommand = pair.Key;
            activeCommand.OnApply(applyContext, pair.Value);
        }
    }

    public void DoRemoveEffect(EffectState activeEffect)
    {
        activeEffect.Context.ExecutionHint = ExecutionHint.RemoveEffect;
        activeEffect.Context.Self.ClearEffect(activeEffect);
        activeEffect.Effect.RemoveChain?.Execute(activeEffect.Context);

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

    public VehicleCalldownRequest TryConsumeVehicleCalldownRequest(ulong entityId)
    {
        return PlayerVehicleCalldownRequests.Remove(entityId, out var result) ? result : null;
    }

    public DeployableCalldownRequest TryConsumeDeployableCalldownRequest(ulong entityId)
    {
        return PlayerDeployableCalldownRequests.Remove(entityId, out var result) ? result : null;
    }

    public ResourceNodeBeaconCalldownRequest TryConsumeResourceNodeBeaconCalldownRequest(ulong entityId)
    {
        return PlayerThumperCalldownRequests.Remove(entityId, out var result) ? result : null;
    }

    public void HandleVehicleCalldownRequest(ulong entityId, VehicleCalldownRequest request)
    {
        if (PlayerVehicleCalldownRequests.ContainsKey(entityId))
        {
            _logger.Information("Discarded an unconsumed vehicle calldown request for {entityId}", entityId);
            PlayerVehicleCalldownRequests.Remove(entityId);
        }
        
        PlayerVehicleCalldownRequests.Add(entityId, request);
    }

    public void HandleDeployableCalldownRequest(ulong entityId, DeployableCalldownRequest request)
    {
        if (PlayerDeployableCalldownRequests.ContainsKey(entityId))
        {
            _logger.Information("Discarded an unconsumed deployable calldown request for {entityId}", entityId);
            PlayerDeployableCalldownRequests.Remove(entityId);
        }
        
        PlayerDeployableCalldownRequests.Add(entityId, request);
    }

    public void HandleResourceNodeBeaconCalldownRequest(ulong entityId, ResourceNodeBeaconCalldownRequest request)
    {
        if (PlayerThumperCalldownRequests.ContainsKey(entityId))
        {
            _logger.Information("Discarded an unconsumed thumper calldown request for {entityId}", entityId);
            PlayerThumperCalldownRequests.Remove(entityId);
        }
        
        PlayerThumperCalldownRequests.Add(entityId, request);
    }

    public void HandleLocalProximityAbilitySuccess(IShard shard, IAptitudeTarget source, uint commandId, uint time, AptitudeTargets targets, Guid? executionId = null)
    {
        var execId = executionId ?? Guid.NewGuid();
        using var logContext = Serilog.Context.LogContext.PushProperty("ExecutionId", execId);
        _logger.Information("HandleLocalProximityAbilitySuccess Source {source}, Command {commandId}, Time {time}, TargetsCount {targetsCount}", source, commandId, time, targets.Count);

        var commandDef = SDBInterface.GetRegisterClientProximityCommandDef(commandId);

        if (commandDef.AbilityId != 0)
        {
            HandleActivateAbility(shard, source, commandDef.AbilityId, time, targets, execId);
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
                ExecutionHint = ExecutionHint.Proximity
            });
        }
    }

    public void HandleActivateAbility(IShard shard, IAptitudeTarget initiator, uint abilityId, uint activationTime, AptitudeTargets targets, Guid? executionId = null)
    {
        var execId = executionId ?? Guid.NewGuid();
        using var logContext = Serilog.Context.LogContext.PushProperty("ExecutionId", execId);
        var chainId = SDBInterface.GetAbilityData(abilityId).Chain;
        if (chainId == 0)
        {
            return;
        }

        _logger.Information("HandleActivateAbility: Ability {AbilityId} starting Chain {ChainId}", abilityId, chainId);

        var chain = Factory.LoadChain(chainId);
        chain.Execute(new Context(shard, initiator)
        {
            ExecutionId = execId,
            ChainId = chainId,
            AbilityId = abilityId,
            Targets = targets,
            InitTime = activationTime,
            ExecutionHint = ExecutionHint.Ability
        });
    }

    public void HandleActivateAbility(IShard shard, IAptitudeTarget initiator, uint abilityId)
    {
        HandleActivateAbility(shard, initiator, abilityId, Shard.CurrentTime, new AptitudeTargets());
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
}