using System;
using System.Collections.Generic;
using System.Numerics;
using AeroMessages.Common;
using AeroMessages.GSS.V66;

namespace GameServer.Aptitude;

public interface IAptitudeTarget
{
    public ulong EntityId { get; } // From BaseEntity
    public EntityId AeroEntityId { get; } // From BaseEntity
    public IShard Shard { get; } // From BaseEntity
    public Vector3 Position { get; } // From BaseEntity

    public List<EffectState> GetActiveEffects();
    public EffectState AddEffect(Effect effect, Context context);
    public void ClearEffect(EffectState state);
    public void SetStatusEffect(byte index, ushort time, StatusEffectData data);
    public void ClearStatusEffect(byte index, ushort time, uint debugEffectId);
}