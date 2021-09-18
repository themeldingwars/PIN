using System;
using System.Collections.Concurrent;
using System.Data;

namespace Core.Data
{
    public static class MagicManager
    {
        private static ConcurrentDictionary<Type, Func<IDataRecord, IRowView>> converters;

        public static T GetActiveRecord<T>(IDataRecord rec)
            where T : IRowView
        {
            if (converters == null)
            {
                converters = new ConcurrentDictionary<Type, Func<IDataRecord, IRowView>>();
            }

            var t = typeof(T);
            Func<IDataRecord, IRowView> c;
            if (!converters.ContainsKey(t))
            {
                c = converters.AddOrUpdate(t, BuildConverter<T>(), (t2, nc) => nc);
            }
            else
            {
                c = converters[t];
            }

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
}