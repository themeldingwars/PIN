using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class CopyInitiationPositionCommand : Command, ICommand
{
    private CopyInitiationPositionCommandDef Params;

    public CopyInitiationPositionCommand(CopyInitiationPositionCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}