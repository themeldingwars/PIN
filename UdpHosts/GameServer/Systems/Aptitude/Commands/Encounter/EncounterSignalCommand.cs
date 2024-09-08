using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class EncounterSignalCommand : Command, ICommand
{
    private EncounterSignalCommandDef Params;

    public EncounterSignalCommand(EncounterSignalCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}