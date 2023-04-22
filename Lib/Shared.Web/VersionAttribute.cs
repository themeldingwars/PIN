using System;

namespace Shared.Web;

[AttributeUsage(AttributeTargets.Method)]
public class VersionAttribute : Attribute
{
    public VersionAttribute(int version)
    {
        Version = version;
    }

    public int Version { get; set; }
}