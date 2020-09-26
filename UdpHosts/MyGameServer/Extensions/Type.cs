using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyGameServer.Extensions {
	public static class TypeExtensions {
		public static T GetAttribute<T>(this Type type, bool inherit = false) where T : Attribute {
			return type.GetCustomAttributes(typeof(T), inherit).FirstOrDefault() as T;
		}
	}
}
