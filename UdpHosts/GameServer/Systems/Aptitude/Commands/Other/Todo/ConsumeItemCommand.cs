using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ConsumeItemCommand : ICommand
{
    private ConsumeItemCommandDef Params;

    public ConsumeItemCommand(ConsumeItemCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}