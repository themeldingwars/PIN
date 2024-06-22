using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class UnpackItemCommand : ICommand
{
    private UnpackItemCommandDef Params;

    public UnpackItemCommand(UnpackItemCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}