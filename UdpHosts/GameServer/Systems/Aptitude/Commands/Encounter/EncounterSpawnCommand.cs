using System;
using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class EncounterSpawnCommand : Command, ICommand
{
    private EncounterSpawnCommandDef Params;

    public EncounterSpawnCommand(EncounterSpawnCommandDef par)
    : base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        Logger.Information("Encounter spawned for {Command} {CommandId}", nameof(EncounterSpawnCommand), Params.Id);

        return true;
    }
}