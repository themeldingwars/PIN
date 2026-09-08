using System.Collections.Generic;
using AeroMessages.GSS;
using GameServer.Entities.Character;
using GameServer.Systems.Aptitude;

namespace GameServer.Entities;

public abstract class BaseAptitudeEntity : BaseEntity, IAptitudeTarget
{
    public const byte MaxEffectCount = 32;
    public const byte InvalidIndex = 255;

    protected EffectState[] ActiveEffects = new EffectState[MaxEffectCount];

    public BaseAptitudeEntity(IShard shard, ulong eid, CharacterEntity owner = null)
    : base(shard, eid)
    {
        Owner = owner;
    }

    public CharacterEntity Owner { get; }

    public List<EffectState> GetActiveEffects() => [.. ActiveEffects];

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
                if (ActiveEffects[i].Stacks < effect.MaxStackCount)
                {
                    ActiveEffects[i].Stacks += 1;
                }
                else
                {
                    return new EffectState() { MaxStacksExceeded = true };
                }

                return ActiveEffects[i];
            }
        }

        if (firstFreeIndex == InvalidIndex)
        {
            // fail!
            Logger.Warning("AddEffect but there are too many active effects!");
            firstFreeIndex = 31; // Lets not crash
        }

        // Keep lifetime and replication clocks separate. InitTime identifies the event that applied the
        // effect (including the client's UseScope timestamp); the server starts this effect's duration now.
        context.EffectStartTime = Shard.CurrentTime;
        var state = new EffectState
        {
            Effect = effect,
            Context = context,
            Time = context.InitTime,
            LastUpdateTime = Shard.CurrentTimeLong,
            Stacks = 1,
            Index = firstFreeIndex
        };

        ActiveEffects[firstFreeIndex] = state;

        // Preserve the event timestamp on the wire, including directly predicted ADS applications.
        var time = unchecked((ushort)state.Time);
        var data = new StatusEffectData
        {
            Id = state.Effect.Id,
            Stack = state.Stacks,
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
        if (state == null || state.Index >= MaxEffectCount || !ReferenceEquals(ActiveEffects[state.Index], state))
        {
            return;
        }

        state.Removed = true;
        ActiveEffects[state.Index] = null;
        var time = unchecked((ushort)state.Context.Shard.CurrentTime);
        ClearStatusEffect(state.Index, time, state.Effect.Id);
        Shard.EntityMan.FlushChanges(this); // Force flush so that we communicate every change
    }

    public abstract void SetStatusEffect(byte index, ushort time, StatusEffectData data);
    public abstract void ClearStatusEffect(byte index, ushort time, uint debugEffectId);
}