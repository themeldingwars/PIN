using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Unlock;

public class UnlockContentCommand : Command, ICommand
{
    private UnlockContentCommandDef Params;

    public UnlockContentCommand(UnlockContentCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}