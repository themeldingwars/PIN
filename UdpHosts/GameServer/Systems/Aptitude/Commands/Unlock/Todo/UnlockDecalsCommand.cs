using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Unlock;

public class UnlockDecalsCommand : Command, ICommand
{
    private UnlockDecalsCommandDef Params;

    public UnlockDecalsCommand(UnlockDecalsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}