using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class BombardmentCommand : ICommand
{
    private BombardmentCommandDef Params;

    public BombardmentCommand(BombardmentCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}