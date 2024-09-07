using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class RemoveEffectByTagCommand : Command, ICommand
{
    private RemoveEffectByTagCommandDef Params;

    public RemoveEffectByTagCommand(RemoveEffectByTagCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}