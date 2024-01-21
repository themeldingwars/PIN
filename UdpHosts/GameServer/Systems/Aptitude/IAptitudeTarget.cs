using System.Collections.Generic;

namespace GameServer.Aptitude;

public interface IAptitudeTarget
{
    public ulong EntityId { get; } // From BaseEntity
    public List<EffectState> GetActiveEffects();
    public EffectState AddEffect(Effect effect, Context context);
    public void ClearEffect(EffectState state);
}