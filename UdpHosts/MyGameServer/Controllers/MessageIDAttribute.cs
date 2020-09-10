using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Controllers {
	public class MessageIDAttribute : Attribute {
		public byte MsgID { get; protected set; }

		public MessageIDAttribute( byte msgID ) {
			MsgID = msgID;
		}
	}
}