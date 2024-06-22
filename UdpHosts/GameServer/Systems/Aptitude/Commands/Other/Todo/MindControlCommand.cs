using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class MindControlCommand : ICommand
{
    private MindControlCommandDef Params;

    public MindControlCommand(MindControlCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}