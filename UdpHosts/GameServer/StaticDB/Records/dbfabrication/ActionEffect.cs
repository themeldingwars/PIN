namespace GameServer.Data.SDB.Records.dbfabrication;
public record class ActionEffect
{
    public float Value2 { get; set; }
    public uint ActionId { get; set; }
    public float Value1 { get; set; }
    public byte Type { get; set; }
}