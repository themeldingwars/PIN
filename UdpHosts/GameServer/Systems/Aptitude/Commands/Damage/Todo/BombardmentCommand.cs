using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class BombardmentCommand : Command, ICommand
{
    private BombardmentCommandDef Params;

    public BombardmentCommand(BombardmentCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}