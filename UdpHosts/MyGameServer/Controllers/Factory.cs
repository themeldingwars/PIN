using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyGameServer.Extensions;

namespace MyGameServer.Controllers {
	public static class Factory {
		public static void Init() {
			_controllers = new ConcurrentDictionary<Enums.GSS.Controllers, Base>();
		}

		private static ConcurrentDictionary<Enums.GSS.Controllers, Base> _controllers;
		public static T Get<T>() where T : Base, new() {
			var attr = typeof(T).GetAttribute<ControllerIDAttribute>();

			if( attr == null )
				throw new ArgumentNullException("T", "Type ["+typeof(T).FullName+"] does not have a ControllerID Attribute.");

			var k = attr.ControllerID;

			if( !_controllers.ContainsKey(k) )
				return _controllers.AddOrUpdate(k, new T(), ( k, nc ) => nc) as T;

			return _controllers[k] as T;
		}

		public static Base Get( Enums.GSS.Controllers controllerID ) {
			if( _controllers.ContainsKey(controllerID) )
				return _controllers[controllerID];

			var t = ForControllerID(controllerID);

			if( t != null )
				return _controllers.AddOrUpdate(controllerID, Activator.CreateInstance(t) as Base, ( k, nc ) => nc);

			return null;
		}

		public static Type ForControllerID( Enums.GSS.Controllers cID ) {
			var ts = ReflectionUtils.FindTypesByAttribute<ControllerIDAttribute>();

			return ts.Where(( t ) => t.GetAttribute<ControllerIDAttribute>().ControllerID == cID).FirstOrDefault();
		}
	}
}
