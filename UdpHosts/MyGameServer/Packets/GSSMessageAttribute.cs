using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Packets {
	public class GSSMessageAttribute : Attribute {
		public Enums.GSS.Controllers? ControllerID { get; protected set; }
		public ulong? EntityID { get; protected set; }
		public byte MsgID { get; protected set; }

		public GSSMessageAttribute( byte mID ) {
			ControllerID = null;
			EntityID = null;
			MsgID = mID;
		}

		public GSSMessageAttribute( Enums.GSS.Controllers cID, byte mID ) {
			ControllerID = cID;
			EntityID = null;
			MsgID = mID;
		}
	}
}
