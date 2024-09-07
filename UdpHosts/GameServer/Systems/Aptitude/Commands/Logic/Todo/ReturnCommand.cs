using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class ReturnCommand : Command, ICommand
{
    private ReturnCommandDef Params;

    public ReturnCommand(ReturnCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}