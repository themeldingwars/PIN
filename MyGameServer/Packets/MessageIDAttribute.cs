using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Packets {
	public class MessageIDAttribute : Attribute {
		public byte IDNumber { get; protected set; }

		public MessageIDAttribute( byte idNum ) {
			IDNumber = idNum;
		}
	}
}