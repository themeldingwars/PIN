using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class RemovePermanentEffectCommand : Command, ICommand
{
    private RemovePermanentEffectCommandDef Params;

    public RemovePermanentEffectCommand(RemovePermanentEffectCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}