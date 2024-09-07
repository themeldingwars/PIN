namespace GameServer.Data.SDB.Records.apt;
public record class LoadRegisterFromLevelCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte LevelData { get; set; }
    public byte FromInitiator { get; set; }
    public byte Regop { get; set; }
}