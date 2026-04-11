using FauFau.Util.CommmonDataTypes;

namespace GameServer.Data.SDB.Records.dbcharacter;
public record class TurretWeapon
{
    public Vector3 PhysicalOrigin { get; set; }
    public string MuzzleHardpoint { get; set; }
    public uint TurretTypeId { get; set; }
    public uint WeaponId { get; set; }
    public uint DamageStatId { get; set; }
    public uint Id { get; set; }
}