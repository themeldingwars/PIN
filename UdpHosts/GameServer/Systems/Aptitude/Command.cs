using GameServer.Data.SDB.Records;

namespace GameServer.Aptitude;

public abstract class Command(ICommandDef par)
{
    public uint Id { get; set; } = par.Id;
}