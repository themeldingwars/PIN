namespace GameServer.Data.SDB.Records.dbencounterdata;
public record class SinCardFields
{
    public int TemplateId { get; set; }
    public string EnumOptions { get; set; }
    public string Name { get; set; }
    public string DefaultValue { get; set; }
    public int DataType { get; set; }
}
