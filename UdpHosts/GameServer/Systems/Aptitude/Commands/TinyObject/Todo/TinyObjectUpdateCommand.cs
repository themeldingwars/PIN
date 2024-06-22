using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class TinyObjectUpdateCommand : ICommand
{
    private TinyObjectUpdateCommandDef Params;

    public TinyObjectUpdateCommand(TinyObjectUpdateCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}