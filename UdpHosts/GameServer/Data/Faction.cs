namespace GameServer.Data;

public class Faction
{
    public byte ID { get; set; }
    public byte Mode { get; set; }

    public static Faction Load(byte id)
    {
        return new Faction { ID = 1, Mode = 1 };
    }
}