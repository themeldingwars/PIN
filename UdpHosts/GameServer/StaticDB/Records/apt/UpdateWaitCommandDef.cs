namespace GameServer.Data.SDB.Records.apt;
public record class UpdateWaitCommandDef
{

    public uint Id { get; set; }
    public uint Duration { get; set; }
    public byte Regop { get; set; }
}