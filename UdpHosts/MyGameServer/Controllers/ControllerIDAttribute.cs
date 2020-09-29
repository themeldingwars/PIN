using System;
using System.Collections.Generic;
using System.Text;

namespace MyGameServer.Controllers {
	public class ControllerIDAttribute : Attribute {
		public Enums.GSS.Controllers ControllerID { get; private set; }

		public ControllerIDAttribute( Enums.GSS.Controllers cID ) {
			ControllerID = cID;
		}
	}
}
