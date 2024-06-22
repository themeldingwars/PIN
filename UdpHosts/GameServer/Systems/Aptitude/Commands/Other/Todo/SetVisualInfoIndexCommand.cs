using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SetVisualInfoIndexCommand : ICommand
{
    private SetVisualInfoIndexCommandDef Params;

    public SetVisualInfoIndexCommand(SetVisualInfoIndexCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}