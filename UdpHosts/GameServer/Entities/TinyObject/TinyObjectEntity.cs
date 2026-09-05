using System.Numerics;
using AeroMessages.Common;
using AeroMessages.GSS;
using GameServer.Systems.Aptitude;
namespace GameServer.Entities.TinyObject;

public sealed class TinyObjectEntity : BaseAptitudeEntity, IAptitudeTarget
{
    public TinyObjectEntity(IShard shard, ulong eid, StaticDB.Records.dbcharacter.TinyObject info, Vector3 position, HostilityInfoData hostility, IEntity container, uint containerIndex)
        : base(shard, eid, owner: null)
    {
        AeroEntityId = new EntityId() { Backing = EntityId, ControllerId = Controller.TinyObjectType };
        Info = info;
        Data = new()
        {
            TypeId = (ushort)info.Id,
            Position = position,
            HostilityInfo = hostility,
        };
        Container = container;
        ContainerIndex = containerIndex;

        if (Info.PosefileId != 0)
        {
            Collision = new()
            {
                HitboxCollisionId = Info.PosefileId,
            };
        }

        if (Info.SpawnStatusfxId != 0)
        {
            SpawnEffectId = Info.SpawnStatusfxId;
        }

        if (Info.HitStatusfxId != 0)
        {
            HitEffectId = Info.HitStatusfxId;
        }
    }

    public StaticDB.Records.dbcharacter.TinyObject Info { get; private set; }
    public TinyObjectData Data { get; private set; }

    public IEntity Container { get; private set; }
    public uint ContainerIndex { get; private set; }

    public uint SpawnEffectId { get; private set; }
    public uint HitEffectId { get; private set; }

    public ushort StatusEffectsChangeTime_0 { get; set; }
    public ushort StatusEffectsChangeTime_1 { get; set; }
    public ushort StatusEffectsChangeTime_2 { get; set; }
    public ushort StatusEffectsChangeTime_3 { get; set; }
    public ushort StatusEffectsChangeTime_4 { get; set; }
    public ushort StatusEffectsChangeTime_5 { get; set; }
    public ushort StatusEffectsChangeTime_6 { get; set; }
    public ushort StatusEffectsChangeTime_7 { get; set; }
    public ushort StatusEffectsChangeTime_8 { get; set; }
    public ushort StatusEffectsChangeTime_9 { get; set; }
    public ushort StatusEffectsChangeTime_10 { get; set; }
    public ushort StatusEffectsChangeTime_11 { get; set; }
    public ushort StatusEffectsChangeTime_12 { get; set; }
    public ushort StatusEffectsChangeTime_13 { get; set; }
    public ushort StatusEffectsChangeTime_14 { get; set; }
    public ushort StatusEffectsChangeTime_15 { get; set; }
    public ushort StatusEffectsChangeTime_16 { get; set; }
    public ushort StatusEffectsChangeTime_17 { get; set; }
    public ushort StatusEffectsChangeTime_18 { get; set; }
    public ushort StatusEffectsChangeTime_19 { get; set; }
    public ushort StatusEffectsChangeTime_20 { get; set; }
    public ushort StatusEffectsChangeTime_21 { get; set; }
    public ushort StatusEffectsChangeTime_22 { get; set; }
    public ushort StatusEffectsChangeTime_23 { get; set; }
    public ushort StatusEffectsChangeTime_24 { get; set; }
    public ushort StatusEffectsChangeTime_25 { get; set; }
    public ushort StatusEffectsChangeTime_26 { get; set; }
    public ushort StatusEffectsChangeTime_27 { get; set; }
    public ushort StatusEffectsChangeTime_28 { get; set; }
    public ushort StatusEffectsChangeTime_29 { get; set; }
    public ushort StatusEffectsChangeTime_30 { get; set; }
    public ushort StatusEffectsChangeTime_31 { get; set; }
    public StatusEffectData? StatusEffects_0 { get; set; }
    public StatusEffectData? StatusEffects_1 { get; set; }
    public StatusEffectData? StatusEffects_2 { get; set; }
    public StatusEffectData? StatusEffects_3 { get; set; }
    public StatusEffectData? StatusEffects_4 { get; set; }
    public StatusEffectData? StatusEffects_5 { get; set; }
    public StatusEffectData? StatusEffects_6 { get; set; }
    public StatusEffectData? StatusEffects_7 { get; set; }
    public StatusEffectData? StatusEffects_8 { get; set; }
    public StatusEffectData? StatusEffects_9 { get; set; }
    public StatusEffectData? StatusEffects_10 { get; set; }
    public StatusEffectData? StatusEffects_11 { get; set; }
    public StatusEffectData? StatusEffects_12 { get; set; }
    public StatusEffectData? StatusEffects_13 { get; set; }
    public StatusEffectData? StatusEffects_14 { get; set; }
    public StatusEffectData? StatusEffects_15 { get; set; }
    public StatusEffectData? StatusEffects_16 { get; set; }
    public StatusEffectData? StatusEffects_17 { get; set; }
    public StatusEffectData? StatusEffects_18 { get; set; }
    public StatusEffectData? StatusEffects_19 { get; set; }
    public StatusEffectData? StatusEffects_20 { get; set; }
    public StatusEffectData? StatusEffects_21 { get; set; }
    public StatusEffectData? StatusEffects_22 { get; set; }
    public StatusEffectData? StatusEffects_23 { get; set; }
    public StatusEffectData? StatusEffects_24 { get; set; }
    public StatusEffectData? StatusEffects_25 { get; set; }
    public StatusEffectData? StatusEffects_26 { get; set; }
    public StatusEffectData? StatusEffects_27 { get; set; }
    public StatusEffectData? StatusEffects_28 { get; set; }
    public StatusEffectData? StatusEffects_29 { get; set; }
    public StatusEffectData? StatusEffects_30 { get; set; }
    public StatusEffectData? StatusEffects_31 { get; set; }

    public override void SetStatusEffect(byte index, ushort time, StatusEffectData data)
    {
        Logger.Debug("Carryable.SetStatusEffect Index {index}, Time {time}, Id {effectId}", index, time, data.Id);

        // Member
        GetType().GetProperty($"StatusEffectsChangeTime_{index}").SetValue(this, time, null);
        GetType().GetProperty($"StatusEffects_{index}").SetValue(this, data, null);
    }

    public override void ClearStatusEffect(byte index, ushort time, uint debugEffectId)
    {
        Logger.Debug("Carryable.ClearStatusEffect Index {index}, Time {time}, Id {effectId}", index, time, debugEffectId);

        // Member
        GetType().GetProperty($"StatusEffectsChangeTime_{index}").SetValue(this, time, null);
        GetType().GetProperty($"StatusEffects_{index}").SetValue(this, null, null);
    }
}