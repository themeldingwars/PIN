using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Packets {
	public class GSSMessageAttribute : Attribute {
		public byte? ControllerID { get; protected set; }
		public ulong? EntityID { get; protected set; }
		public byte MsgID { get; protected set; }

		public GSSMessageAttribute( byte mID ) {
			MsgID = mID;
		}

		public GSSMessageAttribute( byte cID, ulong eID, byte mID ) {
			ControllerID = cID;
			EntityID = eID;
			MsgID = mID;
		}
	}
}
