namespace GameServer.Data.SDB.Records.aptfs;
public record class FireUiEventCommandDef
{
    public string ComponentFunction { get; set; }
    public string ComponentName { get; set; }
    public string GlobalEventName { get; set; }
    public string ComponentArgs { get; set; }
    public uint Id { get; set; }
    public byte GlobalEventShow { get; set; }
}
