using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TauntCommand : ICommand
{
    private TauntCommandDef Params;

    public TauntCommand(TauntCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}