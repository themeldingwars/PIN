namespace GameServer.Data.SDB.Records.customdata;
public record class SetGliderParametersCommandDef
{
    public uint Id { get; set; }
    public uint? Value { get; set; }
}