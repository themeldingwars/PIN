namespace GameServer.Data.SDB.Records.apt;
public record class UpdateWaitAndFireOnceCommandDef
{
    public uint Chain { get; set; }
    public uint Duration { get; set; }    
    public uint Id { get; set; }
    public byte Regop { get; set; }
}