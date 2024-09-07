using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class ImpactMarkInteractivesCommand : Command, ICommand
{
    private ImpactMarkInteractivesCommandDef Params;

    public ImpactMarkInteractivesCommand(ImpactMarkInteractivesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}