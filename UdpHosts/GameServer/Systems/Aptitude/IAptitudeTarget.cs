using AeroMessages.GSS.V66;
using System;
using System.Collections.Generic;

namespace GameServer.Aptitude;

public interface IAptitudeTarget
{
    public ulong EntityId { get; } // From BaseEntity
    public IShard Shard { get; } // From BaseEntity

    public List<EffectState> GetActiveEffects();
    public EffectState AddEffect(Effect effect, Context context);
    public void ClearEffect(EffectState state);
    public void SetStatusEffect(byte index, ushort time, StatusEffectData data);
    public void ClearStatusEffect(byte index, ushort time, uint debugEffectId);
}