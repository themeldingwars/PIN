using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Controllers {
	public class ControllerIDAttribute : Attribute {
		public Packets.GSS.Controllers ControllerID { get; private set; }

		public ControllerIDAttribute( Packets.GSS.Controllers cID ) {
			ControllerID = cID;
		}
	}
}
