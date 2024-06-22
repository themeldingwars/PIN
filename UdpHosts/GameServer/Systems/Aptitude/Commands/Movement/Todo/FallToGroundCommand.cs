using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class FallToGroundCommand : ICommand
{
    private FallToGroundCommandDef Params;

    public FallToGroundCommand(FallToGroundCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}