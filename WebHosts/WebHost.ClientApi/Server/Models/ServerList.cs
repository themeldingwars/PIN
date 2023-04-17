using System.Collections.Generic;

namespace WebHost.ClientApi.Server.Models;

public class ServerList
{
    public IEnumerable<long> ZoneList { get; set; }
}