namespace GameServer.Data.SDB.Records.aptfs;
public record class TargetCurrentVehicleCommandDef : ICommandDef
{
    public uint Id { get; set; }
    public byte FailNone { get; set; }
}
