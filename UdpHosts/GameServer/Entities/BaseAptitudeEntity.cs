using System.Collections.Generic;
using System.Linq;
using AeroMessages.GSS;
using AeroMessages.GSS.Character.Event;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.Protocol;
using GameServer.Systems.Aptitude;
using GameServer.Systems.Aptitude.NamedVars;
using Serilog.Events;

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
        NamedVars = new NamedVar($"{GetType().Name} ({eid:X8})");
    }

    public CharacterEntity Owner { get; }

    public NamedVar NamedVars { get; }

    // Removed since the last ability tick, cancelled at the end of the next one
    public List<EffectState> RemovedEffects { get; } = [];

    public List<EffectState> GetActiveEffects() => [.. ActiveEffects];

    public override string ToString()
    {
        return $"{GetType().Name} ({EntityId:X8})";
    }

    public void FlushUiNamedVariables()
    {
        if (!NamedVars.HasUiChanges)
        {
            return;
        }

        if (this is not CharacterEntity { IsPlayerControlled: true } character)
        {
            NamedVars.FlushUi((key, value) => Logger.Debug("{Entity} has a watched named var {Key} = {Value} but no client of its own, not sending it", this, key, value));
            return;
        }

        var channel = character.Player.NetChannels[ChannelType.ReliableGss];
        NamedVars.FlushUi((key, value) => channel.SendMessage(new UiNamedVariableUpdate { VariableId = key.ToStoreKey(), Value = value }, character.EntityId));
    }

    public EffectState AddEffect(Effect effect, Context context)
    {
        byte firstFreeIndex = InvalidIndex;
        var usedStackIndexes = new HashSet<byte>();
        for (byte i = 0; i < MaxEffectCount; i++)
        {
            if (ActiveEffects[i] == null)
            {
                if (firstFreeIndex == InvalidIndex)
                {
                    firstFreeIndex = i;
                }
            }
            else if (ActiveEffects[i].Effect.Id == effect.Id)
            {
                usedStackIndexes.Add(ActiveEffects[i].StackIndex);
            }
        }

        if (usedStackIndexes.Count >= System.Math.Max(1, (int)effect.MaxStackCount))
        {
            Logger.Verbose("{Entity} effect {EffectId} already has {Stacks}/{MaxStacks} stacks, application dropped", this, effect.Id, usedStackIndexes.Count, effect.MaxStackCount);
            return new EffectState() { MaxStacksExceeded = true };
        }

        byte stackIndex = 0;
        while (usedStackIndexes.Contains(stackIndex))
        {
            stackIndex++;
        }

        if (firstFreeIndex == InvalidIndex)
        {
            // fail!
            Logger.Warning("AddEffect but there are too many active effects!");
            firstFreeIndex = 31; // Lets not crash
        }

        if (context.InitTime == null)
        {
            Logger.Warning("AddEffect {EffectId} with a context that isn't initiated, initiating it now", effect.Id);
            context.InitTime = context.CurrentTime;
        }

        var state = new EffectState
        {
            Effect = effect,
            Context = context,

            // The client matches the effects it predicted by their init time
            Time = context.InitTime.Value,
            StackIndex = stackIndex,
            Index = firstFreeIndex
        };

        ActiveEffects[firstFreeIndex] = state;

        var time = unchecked((ushort)state.Time);
        var data = new StatusEffectData
        {
            Id = state.Effect.Id,
            Stack = state.StackIndex,
            Initiator = state.Context.Initiator.AeroEntityId,
            Time = state.Time,
            MoreDataFlag = 0
        };
        var index = state.Index;
        SetStatusEffect(index, time, data);
        Shard.EntityMan.FlushChanges(this); // Force flush so that we communicate every change

        LogEffectSlot(effect, state, "added");

        return state;
    }

    public void ClearEffect(EffectState state)
    {
        ActiveEffects[state.Index] = null;
        var time = unchecked((ushort)state.Context.CurrentTime);
        ClearStatusEffect(state.Index, time, state.Effect.Id);
        Shard.EntityMan.FlushChanges(this); // Force flush so that we communicate every change

        LogEffectSlot(state.Effect, state, "cleared");
    }

    public virtual float GetCurrentStatModifierValue(AptitudeStat stat)
    {
        return FoldEffectStatModifiers(stat, 1.0f);
    }

    public virtual void RefreshStatModifier(AptitudeStat stat)
    {
    }

    public abstract void SetStatusEffect(byte index, ushort time, StatusEffectData data);
    public abstract void ClearStatusEffect(byte index, ushort time, uint debugEffectId);

    /// <summary>
    ///     Logs one effect slot changing along with what is left on the character afterwards.
    /// </summary>
    /// <param name="effect">The effect whose slot changed.</param>
    /// <param name="state">The state of the slot after the change.</param>
    /// <param name="what">What happened to the slot.</param>
    protected static float FoldStatModifier(float value, ActiveStatModifier mod)
    {
        if (mod.HasCap)
        {
            if (mod.Cap > value)
            {
                value = (value * mod.Multi) + mod.Add;
                if (value >= mod.Cap)
                {
                    value = mod.Cap;
                }
            }
        }
        else
        {
            value = (value * mod.Multi) + mod.Add;
        }

        return value;
    }

    protected float FoldEffectStatModifiers(AptitudeStat stat, float value)
    {
        foreach (var effect in GetActiveEffects())
        {
            if (effect is { Context: { } ctx } && ctx.StatChangelist.TryGetValue(stat, out var mod))
            {
                value = FoldStatModifier(value, mod);
            }
        }

        return value;
    }

    private void LogEffectSlot(Effect effect, EffectState state, string what)
    {
        var holdsStats = state.Context?.StatChangelist.Count > 0;

        // An effect carrying stat modifiers is logged at Debug, since those are the ones a battleframe switch
        // is expected to hand back; the rest is only worth seeing at Verbose so that a firefight does not bury
        // the entries a switch is being read in.
        var level = holdsStats ? LogEventLevel.Debug : LogEventLevel.Verbose;

        if (!Logger.IsEnabled(level))
        {
            return;
        }

        Logger.Write(
            level,
            "{Entity} effect {What}: {EffectId} in slot {Index} (stack {Stack} of {MaxStacks}, {DurationChain} duration chain) holding {Stats}; {EffectCount} active effects",
            this,
            what,
            effect.Id,
            state.Index,
            state.StackIndex,
            effect.MaxStackCount,
            effect.DurationChain == null ? "no" : "a",
            holdsStats ? string.Join(", ", state.Context.StatChangelist.Keys) : "no stats",
            ActiveEffects.Count(existing => existing != null));
    }
}