using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Test {
	public static class DataUtils {
		private static ConcurrentDictionary<uint, Data.Zone> _zones;
		public static Data.Zone GetZone( uint id ) {
			if( _zones == null ) {
				lock( _zones ) {
					if( _zones == null )
						_zones = new ConcurrentDictionary<uint, Data.Zone>();
				}
			}

			if( _zones.ContainsKey(id) )
				return _zones[id];

			if( id != 448 )
				return null;

			var ret = new Data.Zone();
			ret.ID = id;
			ret.Name = "New Eden";
			ret.Timestamp = 1461290346895u;

			ret.POIs.Add("origin", new System.Numerics.Vector3(0,0,0));
			ret.POIs.Add("watchtower", new System.Numerics.Vector3(176.65f, 250.13f, 491.93f));
			ret.POIs.Add("jacuzzi", new System.Numerics.Vector3(-532.0f, -469.0f, 473.0f));

			_zones.AddOrUpdate(id, ret, ( k, nc ) => nc);

			return ret;
		}
	}
}
