namespace GameServer.Data.SDB.Records.apt;
public record class StatusEffectData
{
    public uint NameId { get; set; }
    public uint ApplyChain { get; set; }
    public uint TooltipId { get; set; }
    public uint RemoveChain { get; set; }
    public uint UpdateChain { get; set; }
    public uint DurationChain { get; set; }
    public uint IconId { get; set; }
    public uint UpdateFrequency { get; set; }
    public uint Id { get; set; }
    public byte MaxStackCount { get; set; }
    public byte Hidden { get; set; }
    public byte ExposeToUi { get; set; }
}