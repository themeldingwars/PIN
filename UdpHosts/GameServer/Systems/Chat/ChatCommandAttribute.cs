using System;

namespace GameServer.Systems.Chat;

[AttributeUsage(AttributeTargets.Class)]
public class ChatCommandAttribute : Attribute
{
    public ChatCommandAttribute(string description, string usage, params string[] names)
    {
        Description = description;
        Names = names;
        Usage = usage;
    }

    public string Description { get; }
    public string[] Names { get; }
    public string Usage { get; }
}