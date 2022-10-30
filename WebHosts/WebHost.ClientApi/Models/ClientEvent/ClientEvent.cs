using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebHost.ClientApi.Models.ClientEvent
{
    /// <summary>
    /// Raised when something happens on the client, eg. logout
    /// </summary>
    public class ClientEvent
    {
        /// <summary>
        /// Presumably the type of the event, in all observed cases "event"
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        /// The action which was performed within the client
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// The data of the event
        /// </summary>
        public ClientEventData Data { get; set; }
    }
}
