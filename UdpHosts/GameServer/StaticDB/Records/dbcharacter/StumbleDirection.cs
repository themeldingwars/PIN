namespace GameServer.Data.SDB.Records.dbcharacter;
public record class StumbleDirection
{
    // public Vec2 DirectionIn { get; set; }
    // public Vec2 DirectionOut { get; set; }
    public float ThresholdIn { get; set; }
    public uint Duration { get; set; }
    public float Distance { get; set; }
    public uint StumbleId { get; set; }
    public byte AnimSubstate { get; set; }
}