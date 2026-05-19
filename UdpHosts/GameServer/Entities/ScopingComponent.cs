namespace GameServer.Entities;

public class ScopingComponent
{
    public bool Global { get; set; }
    public float Range { get; set; } = 100f;
}