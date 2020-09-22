using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Entities {
	public class BaseEntity : IEntity {
		public ulong EntityID { get; }
		public IShard Owner { get; }
		public IDictionary<Enums.GSS.Controllers, ushort> ControllerRefMap { get; }

		public BaseEntity( IShard owner, ulong id ) {
			Owner = owner;
			EntityID = id;
			ControllerRefMap = new ConcurrentDictionary<Enums.GSS.Controllers, ushort>();
		}

		public void RegisterController(Enums.GSS.Controllers controller) {
			ControllerRefMap.Add(controller, Owner.AssignNewRefId(this, controller));
		}
	}
}
