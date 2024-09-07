using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class TargetInteractivesCommand : Command, ICommand
{
    private TargetInteractivesCommandDef Params;

    public TargetInteractivesCommand(TargetInteractivesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}