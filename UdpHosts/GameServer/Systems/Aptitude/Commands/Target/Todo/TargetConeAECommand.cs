using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class TargetConeAECommand : Command, ICommand
{
    private TargetConeAECommandDef Params;

    public TargetConeAECommand(TargetConeAECommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}