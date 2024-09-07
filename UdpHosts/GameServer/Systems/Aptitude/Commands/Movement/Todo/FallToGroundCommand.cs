using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class FallToGroundCommand : Command, ICommand
{
    private FallToGroundCommandDef Params;

    public FallToGroundCommand(FallToGroundCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}