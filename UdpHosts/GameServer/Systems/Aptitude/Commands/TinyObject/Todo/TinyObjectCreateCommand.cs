using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TinyObjectCreateCommand : ICommand
{
    private TinyObjectCreateCommandDef Params;

    public TinyObjectCreateCommand(TinyObjectCreateCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}