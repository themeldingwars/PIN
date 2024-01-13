namespace GameServer.Data.SDB.Records.dbvisualrecords;
public record class OrnamentsMapGroups
{
    public uint DisplayFlags { get; set; }
    public float DisplayAngle { get; set; }
    public uint SlotRestrictions { get; set; }
    public uint Id { get; set; }
    public uint LocalizedNameId { get; set; }
    public byte Usage { get; set; }
}
