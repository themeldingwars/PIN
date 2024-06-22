using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class ReturnCommand : ICommand
{
    private ReturnCommandDef Params;

    public ReturnCommand(ReturnCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}