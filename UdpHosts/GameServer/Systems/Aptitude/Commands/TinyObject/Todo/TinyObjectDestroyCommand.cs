using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TinyObjectDestroyCommand : ICommand
{
    private TinyObjectDestroyCommandDef Params;

    public TinyObjectDestroyCommand(TinyObjectDestroyCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}