using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetByExistsCommand : Command, ICommand
{
    private TargetByExistsCommandDef Params;

    public TargetByExistsCommand(TargetByExistsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}