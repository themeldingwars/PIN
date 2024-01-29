using System.Linq;
using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ImpactRemoveEffectCommand : ICommand
{
    private ImpactRemoveEffectCommandDef Params;

    public ImpactRemoveEffectCommand(ImpactRemoveEffectCommandDef par)
    {
        Params = par;
    }

    // TODO
    public bool Execute(Context context)
    {
        return true;
    }
}