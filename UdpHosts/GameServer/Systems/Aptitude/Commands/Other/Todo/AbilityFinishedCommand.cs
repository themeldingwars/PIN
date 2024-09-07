using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class AbilityFinishedCommand : Command, ICommand
{
    private AbilityFinishedCommandDef Params;

    public AbilityFinishedCommand(AbilityFinishedCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}