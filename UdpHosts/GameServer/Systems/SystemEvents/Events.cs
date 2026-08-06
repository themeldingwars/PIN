using GameServer.Entities;

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