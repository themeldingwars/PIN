using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Interaction;

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