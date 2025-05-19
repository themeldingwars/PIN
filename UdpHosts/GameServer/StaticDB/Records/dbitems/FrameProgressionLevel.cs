namespace GameServer.Data.SDB.Records.dbitems;
public record class FrameProgressionLevel
{
    public uint Level { get; set; }
    public uint OrnamentIdFrameLevel { get; set; }
    public float EmissiveMultiplier { get; set; }
    public uint OrnamentIdBackpackLevel { get; set; }
    public uint XpCost { get; set; }
    public byte PilotTokensGain { get; set; }
    public byte PerkPointsGain { get; set; }
}