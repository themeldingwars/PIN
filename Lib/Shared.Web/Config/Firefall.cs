using System.Collections.Generic;

namespace Shared.Web.Config;

public class Firefall
{
    public Dictionary<string, WebHost> WebHosts { get; set; } = new();
}