using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Encounter;

public class EncounterSpawnCommand : Command, ICommand
{
    private EncounterSpawnCommandDef Params;

    public EncounterSpawnCommand(EncounterSpawnCommandDef par)
    : base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        Logger.Information("Encounter spawned for {Command} {CommandId}", nameof(EncounterSpawnCommand), Params.Id);

        result.SetPass();
        return;
    }
}