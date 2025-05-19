namespace GameServer.Data.SDB.Records.dbcharacter;
public record class FactionRelations
{
    public uint FactionB { get; set; }
    public uint FactionA { get; set; }
    public float ReputationAlliance { get; set; }
    public sbyte HostilityStance { get; set; }
    public byte HostilityBidirectional { get; set; }
}