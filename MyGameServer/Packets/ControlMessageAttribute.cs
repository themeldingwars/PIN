using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Packets {
	public class ControlMessageAttribute : Attribute {
		public ControlPacketType MsgID { get; protected set; }

		public ControlMessageAttribute( ControlPacketType mID ) {
			MsgID = mID;
		}
	}
}
