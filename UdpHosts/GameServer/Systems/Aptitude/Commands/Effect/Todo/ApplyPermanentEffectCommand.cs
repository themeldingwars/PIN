using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ApplyPermanentEffectCommand : Command, ICommand
{
    private ApplyPermanentEffectCommandDef Params;

    public ApplyPermanentEffectCommand(ApplyPermanentEffectCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}