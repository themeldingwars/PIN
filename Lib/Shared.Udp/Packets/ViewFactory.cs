using System;
using System.Collections.Concurrent;

namespace Shared.Udp.Packets;

public static class ViewFactory
{
    private static ConcurrentDictionary<Type, IPacketView> _instances;

    public static void Init()
    {
        _instances = new ConcurrentDictionary<Type, IPacketView>();

        // TODO: Get appropriate interfaces
        var types = Type.EmptyTypes;
        foreach (var t in types)
        {
            AddPacketView(t);
        }
    }

    public static T Get<T>()
        where T : IPacketView
    {
        return (T)Get(typeof(T));
    }

    public static IPacketView Get(Type t)
    {
        return _instances[t];
    }

    private static void AddPacketView(Type t)
    {
        // TODO: Runtime generation of BasePacketView subclasses that implement user defined Interfaces
    }
}