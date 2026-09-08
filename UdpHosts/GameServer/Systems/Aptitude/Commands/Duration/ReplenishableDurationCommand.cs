using System.Runtime.CompilerServices;
using GameServer.Extensions;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Duration;

/// <summary>
/// Duration command of a "replenishable" effect: the effect lives for the amount of time the chain around it
/// puts into the register (the boomerang loads its <c>Boomerang Duration</c> item stat there) and expires once
/// that time has passed.
///
/// The command def table of this type has no decoded fields, so this used to be a placeholder that always
/// returned true. A duration chain that can never fail means the effect is never removed: the ability system
/// re-evaluates it on every update tick forever, which is what left the boomerang looping its duration chain
/// (and its sound) for as long as the character stayed in the shard.
/// </summary>
public class ReplenishableDurationCommand : Command, ICommand
{
    /// <summary>
    /// Fallback lifetime for a replenishable effect whose chain never puts a duration into the register.
    /// Without it such an effect would live forever, which is the bug this command exists to prevent.
    /// </summary>
    private const uint DefaultDurationMs = 30_000;

    private static readonly ConditionalWeakTable<Context, StartTime> _starts = new();

    private ReplenishableDurationCommandDef Params;

    public ReplenishableDurationCommand(ReplenishableDurationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var start = _starts.GetValue(context, _ => new StartTime { Value = context.Shard.CurrentTime });
        uint elapsed = unchecked(context.Shard.CurrentTime - start.Value);
        uint duration = ResolveDurationMs(context.Register);

        if (elapsed < duration)
        {
            return true;
        }

        if (OnceLog.ShouldLog((nameof(ReplenishableDurationCommand), Params.Id)))
        {
            Logger.Debug(
                "[{Command} {CommandId}] effect expired after {Elapsed} ms (duration {Duration} ms)",
                nameof(ReplenishableDurationCommand),
                Params.Id,
                elapsed,
                duration);
        }

        // Let the effect be removed and forget the bookkeeping for this context.
        _starts.Remove(context);

        return false;
    }

    /// <summary>
    /// Item stat durations are authored in seconds (the boomerang's is 2), while a handful of chains push a
    /// millisecond value into the register. Treat anything small as seconds.
    /// </summary>
    private static uint ResolveDurationMs(float register)
    {
        if (float.IsNaN(register) || register <= 0f)
        {
            return DefaultDurationMs;
        }

        return register < 1000f ? (uint)(register * 1000f) : (uint)register;
    }

    private sealed class StartTime
    {
        public uint Value { get; set; }
    }
}
