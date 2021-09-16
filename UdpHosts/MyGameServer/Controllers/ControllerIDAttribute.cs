using System;

namespace MyGameServer.Controllers
{
    public class ControllerIDAttribute : Attribute
    {
        public ControllerIDAttribute(Enums.GSS.Controllers cID)
        {
            ControllerID = cID;
        }

        public Enums.GSS.Controllers ControllerID { get; }
    }
}