namespace GameServer.Data.SDB.Records.dbfabrication;
public record class Ingredient
{
    public uint IngredientId { get; set; }
    
    public uint IngredientGroupId { get; set; }
    
    public uint ItemId { get; set; }
    
    public uint Id { get; set; }
}
