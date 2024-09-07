using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class MindControlCommand : Command, ICommand
{
    private MindControlCommandDef Params;

    public MindControlCommand(MindControlCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}