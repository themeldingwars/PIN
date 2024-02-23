using System;

namespace GameServer.Admin;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ServerCommandAttribute : Attribute
{
    public ServerCommandAttribute(string description, string usage, params string[] names)
    {
        Description = description;
        Names = names;
        Usage = usage;
    }

    public string Description { get; }
    public string[] Names { get; }
    public string Usage { get; }
}