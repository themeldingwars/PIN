using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class EncounterSignalCommand : ICommand
{
    private EncounterSignalCommandDef Params;

    public EncounterSignalCommand(EncounterSignalCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}