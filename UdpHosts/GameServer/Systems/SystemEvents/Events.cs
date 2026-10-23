using System.Numerics;
using GameServer.Entities;
using GameServer.Entities.Deployable;

namespace GameServer.Systems.SystemEvents;

public readonly record struct DebugChatDirectMessageEvent(
    string Message, INetworkClient Target);
public readonly record struct DebugChatBroadcastMessageEvent(
    string Message, IEntity Source);

public readonly record struct EntityDamagedEvent(
    IEntity Target,
    int DamageAmount,
    IEntity Source);

public readonly record struct EntityHealedEvent(
    IEntity Target,
    int HealAmount,
    IEntity Source);

public readonly record struct DeployableDestroyedEvent(
    DeployableEntity Target,
    IEntity Source);

public readonly record struct NpcStuckEvent(
    ulong EntityId,
    Vector3 Position,
    Vector3 Destination);