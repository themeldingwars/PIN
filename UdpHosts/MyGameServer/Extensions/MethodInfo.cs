using System;
using System.Linq;
using System.Reflection;

namespace MyGameServer.Extensions {
	public static class MethodInfoExtensions {
		public static T GetAttribute<T>( this MethodInfo type, bool inherit = false ) where T : Attribute {
			return type.GetCustomAttributes(typeof(T), inherit).FirstOrDefault() as T;
		}
	}
}
