namespace GameServer.Data.SDB.Records.apt;
public record class LoadRegisterFromLevelCommandDef
{
    public uint Id { get; set; }
    public byte LevelData { get; set; }
    public byte FromInitiator { get; set; }
    public byte Regop { get; set; }
}