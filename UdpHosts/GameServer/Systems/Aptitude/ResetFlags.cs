using System;

namespace GameServer.Systems.Aptitude;

[Flags]
public enum ResetFlags
{
    None = 0,
    Apply = 1 << 0,
    Update = 1 << 1,
    Remove = 1 << 2,
}