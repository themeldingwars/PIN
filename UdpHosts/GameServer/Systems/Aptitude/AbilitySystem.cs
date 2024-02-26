using System;
using System.Collections.Generic;
using System.Threading;
using AeroMessages.GSS.V66.Character.Command;
using GameServer.Data.SDB;
using GameServer.Enums;

namespace GameServer.Aptitude;

public class AbilitySystem
{
    public Factory Factory;
    private Shard Shard;
    private Dictionary<ulong, VehicleCalldownRequest> PlayerVehicleCalldownRequests;
    private Dictionary<ulong, DeployableCalldownRequest> PlayerDeployableCalldownRequests;
    private Dictionary<ulong, ResourceNodeBeaconCalldownRequest> PlayerThumperCalldownRequests;

    private ulong LastUpdate = 0;
    private ulong UpdateIntervalMs = 20;

    public AbilitySystem(Shard shard)
    {
        Shard = shard;
        Factory = new Factory();
        PlayerVehicleCalldownRequests = new();
        PlayerDeployableCalldownRequests = new();
        PlayerThumperCalldownRequests = new();
    }

    public static float RegistryOp(float first, float second, Operand op)
    {
        switch (op)
        {
            case Operand.ASSIGN:
                return second;
            case Operand.ADDITIVE:
                return first + second;
            case Operand.MULTIPLICATIVE:
                return first * second;
            case Operand.PERK_DAMAGE_SCALAR:
                Console.WriteLine($"Uncertain RegistryOp {op}");
                return first * second; // TODO: Uncertain
            case Operand.DIVIDE_FIRST_BY_SECOND:
                Console.WriteLine($"Uncertain RegistryOp {op}");
                return first / second;
            case Operand.DIVIDE_SECOND_BY_FIRST:
                Console.WriteLine($"Uncertain RegistryOp {op}");
                return second / first;
            default:
                Console.WriteLine($"Unknown RegistryOp {op}");
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
                if (typeof(IAptitudeTarget).IsAssignableFrom(entity.GetType()))
                {
                    ProcessTarget(entity as IAptitudeTarget);
                }
            }
        }
    }

    public void ProcessTarget(IAptitudeTarget entity)
    {
        var activeEffects = entity.GetActiveEffects();
        foreach (var activeEffect in activeEffects)
        {
            if (activeEffect?.Effect.DurationChain != null)
            {
                activeEffect.Context.ExecutionHint = ExecutionHint.DurationEffect;
                bool durationResult = activeEffect.Effect.DurationChain.Execute(activeEffect.Context);
                if (!durationResult)
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
        target.AddEffect(effect, applyContext);

        effect.ApplyChain?.Execute(applyContext);

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
        if (PlayerVehicleCalldownRequests.ContainsKey(entityId))
        {
            var result = PlayerVehicleCalldownRequests[entityId];
            PlayerVehicleCalldownRequests.Remove(entityId);
            return result;
        }

        return null;
    }

    public DeployableCalldownRequest TryConsumeDeployableCalldownRequest(ulong entityId)
    {
        if (PlayerDeployableCalldownRequests.ContainsKey(entityId))
        {
            var result = PlayerDeployableCalldownRequests[entityId];
            PlayerDeployableCalldownRequests.Remove(entityId);
            return result;
        }

        return null;
    }

    public ResourceNodeBeaconCalldownRequest TryConsumeResourceNodeBeaconCalldownRequest(ulong entityId)
    {
        if (PlayerThumperCalldownRequests.ContainsKey(entityId))
        {
            var result = PlayerThumperCalldownRequests[entityId];
            PlayerThumperCalldownRequests.Remove(entityId);
            return result;
        }

        return null;
    }

    public void HandleVehicleCalldownRequest(ulong entityId, VehicleCalldownRequest request)
    {
        if (PlayerVehicleCalldownRequests.ContainsKey(entityId))
        {
            Console.WriteLine($"Discarded an unconsumed vehicle calldown request");
            PlayerVehicleCalldownRequests.Remove(entityId);
        }
        
        PlayerVehicleCalldownRequests.Add(entityId, request);
    }

    public void HandleDeployableCalldownRequest(ulong entityId, DeployableCalldownRequest request)
    {
        if (PlayerDeployableCalldownRequests.ContainsKey(entityId))
        {
            Console.WriteLine($"Discarded an unconsumed deployable calldown request");
            PlayerDeployableCalldownRequests.Remove(entityId);
        }
        
        PlayerDeployableCalldownRequests.Add(entityId, request);
    }

    public void HandleResourceNodeBeaconCalldownRequest(ulong entityId, ResourceNodeBeaconCalldownRequest request)
    {
        if (PlayerThumperCalldownRequests.ContainsKey(entityId))
        {
            Console.WriteLine($"Discarded an unconsumed thumper calldown request");
            PlayerThumperCalldownRequests.Remove(entityId);
        }
        
        PlayerThumperCalldownRequests.Add(entityId, request);
    }

    public void HandleLocalProximityAbilitySuccess(IShard shard, IAptitudeTarget source, uint commandId, uint time, HashSet<IAptitudeTarget> targets)
    {
        Console.WriteLine($"HandleLocalProximityAbilitySuccess Source {source}, Command {commandId}, Time {time}, Targets {string.Join(Environment.NewLine, targets)} ({targets.Count})");

        var commandDef = SDBInterface.GetRegisterClientProximityCommandDef(commandId);

        if (commandDef.AbilityId != 0)
        {
            HandleActivateAbility(shard, source, commandDef.AbilityId, time, targets);
        }

        if (commandDef.Chain != 0)
        {
            var chain = Factory.LoadChain(commandDef.Chain);
            chain.Execute(new Context(shard, source)
            {
                ChainId = commandDef.Chain,
                Targets = targets,
                InitTime = time,
                ExecutionHint = ExecutionHint.Proximity
            });
        }
    }

    public void HandleActivateAbility(IShard shard, IAptitudeTarget initiator, uint abilityId, uint activationTime, HashSet<IAptitudeTarget> targets)
    {
        var chainId = SDBInterface.GetAbilityData(abilityId).Chain;
        if (chainId == 0)
        {
            return;
        }

        var chain = Factory.LoadChain(chainId);
        chain.Execute(new Context(shard, initiator)
        {
            ChainId = chainId,
            AbilityId = abilityId,
            Targets = targets,
            InitTime = activationTime,
            ExecutionHint = ExecutionHint.Ability
        });
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