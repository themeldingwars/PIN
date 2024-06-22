using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class CopyInitiationPositionCommand : ICommand
{
    private CopyInitiationPositionCommandDef Params;

    public CopyInitiationPositionCommand(CopyInitiationPositionCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}