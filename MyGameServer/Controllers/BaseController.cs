using System;
using System.Collections.Generic;
using System.Linq;

using MyGameServer.Extensions;
using MyGameServer.Packets;

namespace MyGameServer.Controllers {
	public abstract class BaseController {
		public Enums.GSS.Controllers ControllerID { get; private set; }

		protected BaseController() {
			try {
				ControllerID = GetType().GetAttribute<ControllerIDAttribute>().ControllerID;
			} catch {
				throw new MissingMemberException(this.GetType().FullName, "Missing required ControllerID attribute");
			}
		}

		public void HandlePacket(NetworkClient client, ulong EntityID, byte MsgID, GamePacket packet) {
			var method = ReflectionUtils.FindMethodsByAttribute<MessageIDAttribute>(this).Where(( mi ) => mi.GetAttribute<MessageIDAttribute>().MsgID == MsgID).FirstOrDefault();

			if( method == null ) {
				Program.Logger.Verbose("---> Unrecognized MsgID for GSS Packet; Controller = {0} Entity = 0x{1:X8} MsgID = {2}!", ControllerID, EntityID, MsgID);
				return;
			}

			method.Invoke(this, new object[] { client, EntityID, packet });
		}
	}
}
