using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyGameServer.Extensions;

namespace MyGameServer.Controllers {
	public static class ControllerFactory {
		private static ConcurrentDictionary<Enums.GSS.Controllers, BaseController> _controllers;
		public static T Get<T>() where T : BaseController, new() {
			if( _controllers == null )
				_controllers = new ConcurrentDictionary<Enums.GSS.Controllers, BaseController>();

			var attr = typeof(T).GetAttribute<ControllerIDAttribute>();

			if( attr == null )
				throw new ArgumentNullException("T", "Type ["+typeof(T).FullName+"] does not have a ControllerID Attribute.");

			var k = attr.ControllerID;

			if( _controllers.ContainsKey(k) )
				_controllers.AddOrUpdate(k, new T(), ( k, nc ) => nc);

			return _controllers[k] as T;
		}

		public static BaseController Get( Enums.GSS.Controllers controllerID ) {
			if( _controllers == null )
				_controllers = new ConcurrentDictionary<Enums.GSS.Controllers, BaseController>();

			if( _controllers.ContainsKey(controllerID) )
				_controllers.AddOrUpdate(controllerID, Activator.CreateInstance(ForControllerID(controllerID)) as BaseController, ( k, nc ) => nc);

			return _controllers[controllerID];
		}

		public static Type ForControllerID( Enums.GSS.Controllers cID ) {
			return ReflectionUtils.FindTypesByAttribute<ControllerIDAttribute>().Where(( t ) => t.GetAttribute<ControllerIDAttribute>().ControllerID == cID).FirstOrDefault();
		}
	}
}
