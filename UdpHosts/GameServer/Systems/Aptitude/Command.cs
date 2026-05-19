using GameServer.StaticDB.Records;
using Serilog;

namespace GameServer.Systems.Aptitude;

public abstract class Command(ICommandDef par)
{
    protected readonly ILogger Logger = Log.ForContext<Command>();

    public uint Id { get; set; } = par.Id;
}