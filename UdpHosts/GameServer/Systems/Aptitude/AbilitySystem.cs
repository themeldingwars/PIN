using System;
using System.Threading;
using Aero.Gen;
using AeroMessages.Common;
using AeroMessages.GSS.V66.Character.Command;
using AeroMessages.GSS.V66.Character.Event;
using GameServer.Data;
using GameServer.Data.SDB;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Extensions;

namespace GameServer.Aptitude;

public class AbilitySystem
{
    public Factory Factory;
    private Shard Shard;

    private ulong LastUpdate = 0;
    private ulong UpdateIntervalMs = 20;

    public AbilitySystem(Shard shard)
    {
        Shard = shard;
        Factory = new Factory();
        Test();
    }

    public void Test()
    {
        // var context = new Context(Shard, new Entities.Character.Character(Shard, 0))
        // {
        //     ChainId = 67035
        // };
    }

    public void Tick(double deltaTime, ulong currentTime, CancellationToken ct)
    {
        if (currentTime > LastUpdate + UpdateIntervalMs)
        {
            LastUpdate = currentTime;
            foreach (var entity in Shard.Entities.Values)
            {
                // if (entity.GetType() == typeof(IAptitudeTarget))
                // entity.GetType() is IAptitudeTarget
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
        if (effectId == 0) return;
        var effect = Factory.LoadEffect(effectId);
        target.AddEffect(effect, context);
        effect.ApplyChain?.Execute(context);
    }

    public void DoRemoveEffect(EffectState activeEffect)
    {
        activeEffect.Context.Self.ClearEffect(activeEffect);
        activeEffect.Effect.RemoveChain?.Execute(activeEffect.Context);
    }

    public void HandleVehicleCalldownRequest()
    {
        
    }

    public void HandleDeployableCalldownRequest()
    {
        
    }

    public void HandleResourceNodeBeaconCalldownRequest()
    {

    }

    public void HandleLocalProximityAbilitySuccess()
    {

    }

    public void HandleActivateAbility(IShard shard, IAptitudeTarget initiator, uint abilityId, uint activationTime, IAptitudeTarget[] targets)
    {
        var chainId = SDBInterface.GetAbilityData(abilityId).Chain;
        if (chainId == 0) return;
        var chain = Factory.LoadChain(chainId);
        chain.Execute(new Context(shard, initiator)
        {
            ChainId = chainId,
            AbilityId = abilityId,
            Targets = targets,
            InitTime = activationTime,
            // InitPosition = 
        });
    }

    public void HandleTargetAbility()
    {

    }

    public void HandleDeactivateAbility()
    {

    }

    public void HandleActivateConsumable()
    {

    }
}