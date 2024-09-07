using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ConsumeItemCommand : Command, ICommand
{
    private ConsumeItemCommandDef Params;

    public ConsumeItemCommand(ConsumeItemCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}