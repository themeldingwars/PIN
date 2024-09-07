using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class PeekRegisterCommand : Command, ICommand
{
    private PeekRegisterCommandDef Params;

    public PeekRegisterCommand(PeekRegisterCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}