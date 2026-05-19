using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

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