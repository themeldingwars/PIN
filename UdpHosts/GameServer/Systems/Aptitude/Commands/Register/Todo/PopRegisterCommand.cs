using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class PopRegisterCommand : ICommand
{
    private PopRegisterCommandDef Params;

    public PopRegisterCommand(PopRegisterCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}