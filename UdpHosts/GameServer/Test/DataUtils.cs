using GameServer.Data;
using System.Collections.Concurrent;
using System.Numerics;

namespace GameServer.Test
{
    public static class DataUtils
    {
        private static ConcurrentDictionary<uint, Zone> _zones;

        public static void Init()
        {
            _zones = new ConcurrentDictionary<uint, Zone>();
        }

        public static Zone GetZone(uint id)
        {
            if (_zones.ContainsKey(id))
            {
                return _zones[id];
            }

            if (id != 448)
            {
                return null;
            }

            var ret = new Zone();
            ret.ID = id;
            ret.Name = "New Eden";
            ret.Timestamp = 1461290346895u;

            ret.POIs.Add("origin", new Vector3(0, 0, 0));
            ret.POIs.Add("watchtower", new Vector3(176.65f, 250.13f, 491.94f));
            ret.POIs.Add("jacuzzi", new Vector3(-532.0f, -469.0f, 473.0f));

            return _zones.AddOrUpdate(id, ret, (k, nc) => nc);
        }
    }
}