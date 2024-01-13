namespace GameServer.Data.SDB.Records.dbitems;
public record class BattleframeVisuals
{
    public uint HandAnimnetworkId { get; set; }
    public uint HandVisualrecId { get; set; }
    public uint VisualrecId { get; set; }
    public uint VisualGroup { get; set; }
    public uint AnimnetworkId { get; set; }
    public string Gender { get; set; }
    public byte Race { get; set; }
}
