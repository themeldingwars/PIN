using System;
using System.Collections.Generic;

namespace GameServer.Systems.SystemEvents;

public sealed class EventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = [];
    private readonly Queue<object> _eventQueue = new();

    public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
    {
        var type = typeof(TEvent);

        if (!_handlers.TryGetValue(type, out var handlers))
        {
            handlers = [];
            _handlers[type] = handlers;
        }

        handlers.Add(handler);

        return new Subscription(() =>
        {
            handlers.Remove(handler);

            if (handlers.Count == 0)
            {
                _handlers.Remove(type);
            }
        });
    }

    public void Publish<TEvent>(TEvent evt)
    {
        Dispatch(evt);
    }

    public void Enqueue<TEvent>(TEvent evt)
    {
        _eventQueue.Enqueue(evt!);
    }

    public void Flush()
    {
        while (_eventQueue.Count > 0)
        {
            var evt = _eventQueue.Dequeue();

            DispatchDynamic(evt);
        }
    }

    private void Dispatch<TEvent>(TEvent evt)
    {
        var type = typeof(TEvent);

        if (!_handlers.TryGetValue(type, out var handlers))
        {
            return;
        }

        var snapshot = handlers.ToArray();
        foreach (var handler in snapshot)
        {
            ((Action<TEvent>)handler)(evt);
        }
    }

    private void DispatchDynamic(object evt)
    {
        var type = evt.GetType();

        if (!_handlers.TryGetValue(type, out var handlers))
        {
            return;
        }

        var snapshot = handlers.ToArray();
        foreach (var handler in snapshot)
        {
            handler.DynamicInvoke(evt);
        }
    }

    private sealed class Subscription : IDisposable
    {
        private readonly Action _unsubscribe;
        private bool _disposed;

        public Subscription(Action unsubscribe)
        {
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _unsubscribe();
        }
    }
}