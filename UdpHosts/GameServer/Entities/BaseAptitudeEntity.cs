using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AeroMessages.Common;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.Deployable;
using AeroMessages.GSS.V66.Deployable.View;
using GameServer;
using GameServer.Aptitude;
using GameServer.Entities;

namespace GameServer.Entities;

public abstract class BaseAptitudeEntity : BaseEntity, IAptitudeTarget
{
    public const byte MaxEffectCount = 32;
    public const byte InvalidIndex = 255;

    protected EffectState[] ActiveEffects = new EffectState[MaxEffectCount];

    public BaseAptitudeEntity(IShard shard, ulong eid)
    : base(shard, eid)
    {
    }

    public List<EffectState> GetActiveEffects() => ActiveEffects.ToList<EffectState>();

    public override string ToString()
    {
        return $"{GetType().Name} ({EntityId})";
    }

    public EffectState AddEffect(Effect effect, Context context)
    {
        byte firstFreeIndex = InvalidIndex;
        for (byte i = 0; i < MaxEffectCount; i++)
        {
            if (ActiveEffects[i] == null)
            {
                if (firstFreeIndex == InvalidIndex)
                {
                    firstFreeIndex = i;
                }
            }
            
            if (ActiveEffects[i]?.Effect.Id == effect.Id)
            {
                // TODO: What to do?
                if (ActiveEffects[i].Stacks < effect.MaxStackCount)
                { 
                    ActiveEffects[i].Stacks += 1;
                }

                return ActiveEffects[i];
            }
        }
    
        if (firstFreeIndex == InvalidIndex)
        {
            // fail!
            Console.WriteLine($"AddEffect but there are too many active effects!");
            firstFreeIndex = 31; // Lets not crash
        }

        var state = new EffectState
        {
            Effect = effect,
            Context = context,
            Time = Shard.CurrentTime,
            Stacks = 1,
            Index = firstFreeIndex
        };

        ActiveEffects[firstFreeIndex] = state;

        var time = unchecked((ushort)state.Time);
        var data = new StatusEffectData
        {
            Id = state.Effect.Id,
            Initiator = state.Context.Initiator.AeroEntityId,
            Time = state.Time,
            MoreDataFlag = 0
        };
        var index = state.Index;
        SetStatusEffect(index, time, data);
        Shard.EntityMan.FlushChanges(this); // Force flush so that we communicate every change

        return state;
    }
    
    public void ClearEffect(EffectState state)
    {
        ActiveEffects[state.Index] = null;
        var time = unchecked((ushort)state.Context.Shard.CurrentTime);
        ClearStatusEffect(state.Index, time, state.Effect.Id);
        Shard.EntityMan.FlushChanges(this); // Force flush so that we communicate every change
    }

    public abstract void SetStatusEffect(byte index, ushort time, StatusEffectData data);
    public abstract void ClearStatusEffect(byte index, ushort time, uint debugEffectId);
}