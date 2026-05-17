using System;

namespace GameServer.Systems.SystemEvents;

public interface IEventBus
{
    IDisposable Subscribe<TEvent>(Action<TEvent> handler);
    void Publish<TEvent>(TEvent evt);
    void Enqueue<TEvent>(TEvent evt);
    void Flush();
}