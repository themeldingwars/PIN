namespace GameServer.Data.SDB.Records.customdata;

public record ModifyDamageForInflictCommandDef : ICommandDef
{
    public uint Id { get; set; }
}