using System.Collections.Generic;
using System.Linq;
using AeroMessages.GSS;
using GameServer.Entities.Character;
using GameServer.Systems.Aptitude;
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
    }

    public CharacterEntity Owner { get; }

    public List<EffectState> GetActiveEffects() => [.. ActiveEffects];

    public override string ToString()
    {
        return $"{GetType().Name} ({EntityId:X8})";
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
                    LogEffectSlot(effect, ActiveEffects[i], "stacked");
                }
                else
                {
                    // The caller is told to drop the application, so whatever the effect would have applied
                    // this time is lost while the modifier already in the slot keeps counting.
                    LogEffectSlot(effect, ActiveEffects[i], "already at max stacks, application dropped");
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

        LogEffectSlot(effect, state, "added");

        return state;
    }

    public void ClearEffect(EffectState state)
    {
        ActiveEffects[state.Index] = null;
        var time = unchecked((ushort)state.Context.Shard.CurrentTime);
        ClearStatusEffect(state.Index, time, state.Effect.Id);
        Shard.EntityMan.FlushChanges(this); // Force flush so that we communicate every change

        LogEffectSlot(state.Effect, state, "cleared");
    }

    public abstract void SetStatusEffect(byte index, ushort time, StatusEffectData data);
    public abstract void ClearStatusEffect(byte index, ushort time, uint debugEffectId);

    /// <summary>
    ///     Logs one effect slot changing along with what is left on the character afterwards.
    /// </summary>
    /// <param name="effect">The effect whose slot changed.</param>
    /// <param name="state">The state of the slot after the change.</param>
    /// <param name="what">What happened to the slot.</param>
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
            "{Entity} effect {What}: {EffectId} in slot {Index} ({Stacks}/{MaxStacks} stacks, {DurationChain} duration chain) holding {Stats}; {EffectCount} active effects",
            this,
            what,
            effect.Id,
            state.Index,
            state.Stacks,
            effect.MaxStackCount,
            effect.DurationChain == null ? "no" : "a",
            holdsStats ? string.Join(", ", state.Context.StatChangelist.Keys) : "no stats",
            ActiveEffects.Count(existing => existing != null));
    }
}