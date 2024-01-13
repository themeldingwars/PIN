namespace GameServer.Data.SDB.Records.dbitems;
public record class RootItem
{
    public uint NameId { get; set; }
    public uint Flags { get; set; }
    public uint WebIconId { get; set; }
    public uint DescriptionId { get; set; }
    public uint SdbId { get; set; }
    public ushort LootDropVisualRecordId { get; set; }
    public ushort ItemSubtype { get; set; }
    public ushort EliteLevel { get; set; }
    public ushort ModuleTable { get; set; }
    public ushort ClassCertId { get; set; }
    public ushort FactionId { get; set; }
    public ushort SalvageRewards { get; set; }
    public ushort AutogenGroupId { get; set; }
    public ushort ItemSetId { get; set; }
    public ushort CraftingTypeId { get; set; }
    public ushort LootDropVisualGroupId { get; set; }
    public byte LootDropVisualGroupIndex { get; set; }
    public byte Level { get; set; }
    public byte RequiredLevel { get; set; }
    public byte ApprovalState { get; set; }
    public byte DisplayDamageType { get; set; }
    public byte IsReferenced { get; set; }
    public byte Type { get; set; }
    public byte StackSize { get; set; }
    public byte CertificateRequirementsOp { get; set; }
    public byte Quality { get; set; }
    public byte TierId { get; set; }
}