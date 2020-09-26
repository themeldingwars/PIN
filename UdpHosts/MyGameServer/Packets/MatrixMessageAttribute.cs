using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Packets {
	public class MatrixMessageAttribute : Attribute {
		public Enums.MatrixPacketType MsgID { get; protected set; }

		public MatrixMessageAttribute( Enums.MatrixPacketType mID ) {
			MsgID = mID;
		}
	}
}
