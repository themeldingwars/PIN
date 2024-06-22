using System;
using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class EncounterSpawnCommand : ICommand
{
    private EncounterSpawnCommandDef Params;

    public EncounterSpawnCommand(EncounterSpawnCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        Console.WriteLine($"Encounter spawned for EncounterSpawnCommand id {Params.Id}");

        return true;
    }
}