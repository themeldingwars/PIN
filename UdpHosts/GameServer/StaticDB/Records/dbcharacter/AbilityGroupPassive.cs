namespace GameServer.Data.SDB.Records.dbcharacter;
public record class AbilityGroupPassive
{
    public uint Groupid { get; set; }
    public uint Abilityid { get; set; }
    public uint Id { get; set; }
}