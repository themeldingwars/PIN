using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class ImpactMarkInteractivesCommand : ICommand
{
    private ImpactMarkInteractivesCommandDef Params;

    public ImpactMarkInteractivesCommand(ImpactMarkInteractivesCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}