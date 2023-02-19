using GameServer.Extensions;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace GameServer.Controllers;

public static class Factory
{
    private static ConcurrentDictionary<Enums.GSS.Controllers, Base> _controllers;

    public static void Init()
    {
        _controllers = new ConcurrentDictionary<Enums.GSS.Controllers, Base>();
    }

    public static T Get<T>()
        where T : Base, new()
    {
        var attr = typeof(T).GetAttribute<ControllerIDAttribute>();

        if (attr == null)
        {
            throw new ArgumentNullException("T", "Type [" + typeof(T).FullName + "] does not have a ControllerID Attribute.");
        }

        var k = attr.ControllerID;

        if (!_controllers.ContainsKey(k))
        {
            return _controllers.AddOrUpdate(k, new T(), (k, nc) => nc) as T;
        }

        return _controllers[k] as T;
    }

    public static Base Get(Enums.GSS.Controllers controllerId)
    {
        if (_controllers.ContainsKey(controllerId))
        {
            return _controllers[controllerId];
        }

        var t = ForControllerId(controllerId);

        return t != null ? _controllers.AddOrUpdate(controllerId, Activator.CreateInstance(t) as Base, (k, nc) => nc) : null;
    }

    public static Type ForControllerId(Enums.GSS.Controllers controllerId)
    {
        var ts = ReflectionUtils.FindTypesByAttribute<ControllerIDAttribute>();

        return ts.FirstOrDefault(t => t.GetAttribute<ControllerIDAttribute>().ControllerID == controllerId);
    }
}