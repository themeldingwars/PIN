using System.Numerics;
using AeroMessages.GSS.V66.Character.Event;
using GameServer.Entities.Character;

namespace GameServer.Physics;

public struct DebugProjectileHitCallbacks
{
    public Shard Shard;

    public DebugProjectileHitCallbacks(Shard shard)
    {
        Shard = shard;
    }

    public void SendDebugProjectileSpawn(CharacterEntity source, uint traceId, Vector3 origin, Vector3 direction, float speed)
    {
        var rayVector = direction * speed;
        var msg = new TookDebugWeaponHit
        {
            Data = new()
            {
                Time = Shard.CurrentTime,
                TraceType = AeroMessages.GSS.V66.TookDebugWeaponHitData.DebugTraceType.Spawn,
                Unk2_TraceId = traceId,
                Position = origin,
                Direction = rayVector,
            }
        };
        if (source.IsPlayerControlled && source.Player.Preferences.DebugWeapon > 0)
        {
            source.Player.NetChannels[ChannelType.ReliableGss].SendMessage(msg, source.EntityId);
        }
    }

    public void SendDebugProjectileImpact(CharacterEntity source, uint traceId, Vector3 position, Vector3 normal)
    {
        var msg = new TookDebugWeaponHit
        {
            Data = new()
            {
                Time = Shard.CurrentTime,
                TraceType = AeroMessages.GSS.V66.TookDebugWeaponHitData.DebugTraceType.Impact,
                Unk2_TraceId = traceId,
                Position = position,
                Direction = normal,
            }
        };
        if (source.IsPlayerControlled && source.Player.Preferences.DebugWeapon > 0)
        {
            source.Player.NetChannels[ChannelType.ReliableGss].SendMessage(msg, source.EntityId);
        }
    }

    public void SendDebugProjectilePoseHit(CharacterEntity source, uint traceId, Vector3 markerOrigin, Vector3 poseOrigin)
    {
        var msg = new TookDebugWeaponHit
        {
            Data = new()
            {
                Time = Shard.CurrentTime,
                TraceType = AeroMessages.GSS.V66.TookDebugWeaponHitData.DebugTraceType.Posefile_Hit,
                Unk2_TraceId = traceId,
                Position = markerOrigin,
                Direction = new Vector3(0.225f, 0.974f, 0),
                HaveUnk8 = 1,
                Unk8 = new AeroMessages.GSS.V66.TookDebugWeaponHitRelatedData
                {
                    Target = source.AeroEntityId,
                    Origin = poseOrigin,
                    Orientation = Quaternion.Identity,
                    Unk4 = 0,
                    Unk5 = 0xFF,
                },
                HaveRagdoll = 0,
            }
        };
        if (source.IsPlayerControlled && source.Player.Preferences.DebugWeapon > 0)
        {
            source.Player.NetChannels[ChannelType.ReliableGss].SendMessage(msg, source.EntityId);
        }
    }
}