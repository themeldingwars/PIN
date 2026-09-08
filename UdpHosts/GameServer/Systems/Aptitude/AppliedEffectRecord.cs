namespace GameServer.Systems.Aptitude;

/// <summary>
/// A single effect application, remembered by the chain that caused it (see <see cref="Context.AppliedEffects"/>).
/// </summary>
public sealed class AppliedEffectRecord
{
    public AppliedEffectRecord(IAptitudeTarget target, EffectState state)
    {
        Target = target;
        State = state;
    }

    public IAptitudeTarget Target { get; }

    public EffectState State { get; }

    /// <summary>
    /// True while the effect this record was made for is still sitting in the target's effect slots.
    /// </summary>
    public bool IsStillActive()
    {
        if (Target == null || State?.Effect == null)
        {
            return false;
        }

        foreach (var active in Target.GetActiveEffects())
        {
            if (ReferenceEquals(active, State))
            {
                return true;
            }
        }

        return false;
    }
}
