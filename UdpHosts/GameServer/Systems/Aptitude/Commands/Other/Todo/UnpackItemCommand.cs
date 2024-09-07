using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class UnpackItemCommand : Command, ICommand
{
    private UnpackItemCommandDef Params;

    public UnpackItemCommand(UnpackItemCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}