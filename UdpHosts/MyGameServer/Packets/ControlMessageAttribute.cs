using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Packets {
	public class ControlMessageAttribute : Attribute {
		public Enums.ControlPacketType MsgID { get; protected set; }

		public ControlMessageAttribute( Enums.ControlPacketType mID ) {
			MsgID = mID;
		}
	}
}
