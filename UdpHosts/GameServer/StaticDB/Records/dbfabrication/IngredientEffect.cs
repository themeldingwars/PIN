namespace GameServer.Data.SDB.Records.dbfabrication;
public record class IngredientEffect
{
    public uint IngredientId { get; set; }
    public float Value2 { get; set; }
    public uint Id { get; set; }
    public float Value1 { get; set; }
    public uint Type { get; set; }
}