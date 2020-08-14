using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Packets {
	public class MatrixMessageAttribute : Attribute {
		public MatrixPacketType MsgID { get; protected set; }

		public MatrixMessageAttribute( MatrixPacketType mID ) {
			MsgID = mID;
		}
	}
}
