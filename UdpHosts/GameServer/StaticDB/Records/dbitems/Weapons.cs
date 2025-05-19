using System.Collections.Generic;

namespace GameServer.Data.SDB.Records.dbitems;
public record class Weapons
{
    public uint WarpaintPaletteId { get; set; }
    public uint TracerPfxId { get; set; }
    public List<byte> DecalData { get; set; }
    public uint FirstPersonAnimnetId { get; set; }
    public uint BarrelCziMapOverrideId { get; set; }
    public uint OrnamentsMapGroupId1 { get; set; }
    public uint FirstPersonReceiverVisualrecId { get; set; }
    public uint FirstPersonReceiverCziMapOverrideId { get; set; }
    public uint ReceiverVisualrecId { get; set; }
    public uint BarrelVisualrecId { get; set; }
    public uint BuiltinVisualweapon { get; set; }
    public uint ThirdPersonAnimnetId { get; set; }
    public uint WeaponTypeId { get; set; }
    public uint OrnamentsMapGroupId2 { get; set; }
    public uint ReceiverCziMapOverrideId { get; set; }
    public uint FirstPersonBarrelVisualrecId { get; set; }
    public uint FirstPersonBarrelCziMapOverrideId { get; set; }
    public uint Id { get; set; }
    public List<byte> PatternData { get; set; }
}