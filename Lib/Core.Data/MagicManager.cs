using System;
using System.Collections.Concurrent;
using System.Data;

namespace Core.Data;

public static class MagicManager
{
    private static ConcurrentDictionary<Type, Func<IDataRecord, IRowView>> _converters;

    public static T GetActiveRecord<T>(IDataRecord rec)
        where T : IRowView
    {
        _converters ??= new ConcurrentDictionary<Type, Func<IDataRecord, IRowView>>();

        var t = typeof(T);
        var c = !_converters.ContainsKey(t) ? _converters.AddOrUpdate(t, BuildConverter<T>(), (t2, nc) => nc) : _converters[t];

        return (T)c(rec);
    }

    private static Func<IDataRecord, IRowView> BuildConverter<T>()
    {
        return BuildConverter(typeof(T));
    }

    private static Func<IDataRecord, IRowView> BuildConverter(Type t)
    {
        return dr =>
               {
                   var ret = Activator.CreateInstance(t) as IRowView;
                   ret.Load(dr);
                   return ret;
               };
    }
}