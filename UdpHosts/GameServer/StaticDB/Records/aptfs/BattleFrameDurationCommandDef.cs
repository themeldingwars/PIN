namespace GameServer.Data.SDB.Records.aptfs;
public record class BattleFrameDurationCommandDef
{
    public uint Id { get; set; }
    public byte Classtype { get; set; }
    public byte Notchanged { get; set; }
    public byte Negate { get; set; }
}
