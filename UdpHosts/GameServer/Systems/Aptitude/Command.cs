using GameServer.Data.SDB.Records;
using Serilog;

namespace GameServer.Aptitude;

public abstract class Command(ICommandDef par)
{
    protected readonly ILogger Logger = Log.ForContext<Command>();

    public uint Id { get; set; } = par.Id;
}