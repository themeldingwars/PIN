using GameServer.Entities;

namespace GameServer.Systems.SystemEvents;

public readonly record struct DebugChatDirectMessageEvent(
    string Message, INetworkClient Target);
public readonly record struct DebugChatBroadcastMessageEvent(
    string Message, IEntity Source);