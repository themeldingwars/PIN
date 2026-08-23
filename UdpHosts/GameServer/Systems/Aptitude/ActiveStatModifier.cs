using GameServer.Enums;

namespace GameServer.Systems.Aptitude;

public class ActiveStatModifier
{
    public StatModifierIdentifier Stat { get; set; }
    public float Multi { get; set; } = 1.0f;
    public float Add { get; set; }
    public float Cap { get; set; }
    public bool HasCap { get; set; }
}
