using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Unlock;

public class UnlockPatternsCommand : Command, ICommand
{
    private UnlockPatternsCommandDef Params;

    public UnlockPatternsCommand(UnlockPatternsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}