namespace GameServer.StaticDB.Records.dbcharacter;
public record class AbilityGroupPassive
{
    public uint Groupid { get; set; }
    public uint Abilityid { get; set; }
    public uint Id { get; set; }
}