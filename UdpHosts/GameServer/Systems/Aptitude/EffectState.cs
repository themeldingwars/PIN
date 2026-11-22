using System.Linq;

namespace GameServer.Systems.Aptitude;

public static class EffectStateExtensions
{
    public static int CountEffect(this IAptitudeTarget target, uint effectId)
    {
        return target.GetActiveEffects().Count(active => active?.Effect.Id == effectId);
    }

    public static EffectState FirstEffect(this IAptitudeTarget target, uint effectId)
    {
        return target.GetActiveEffects().FirstOrDefault(active => active?.Effect.Id == effectId);
    }
}

public class EffectState
{
    public Effect Effect;
    public Context Context;
    public byte Index;
    public uint Time;
    public byte StackIndex;
    public bool MaxStacksExceeded;
}
