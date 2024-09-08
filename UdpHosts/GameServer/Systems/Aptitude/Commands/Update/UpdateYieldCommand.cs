using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class UpdateYieldCommand : Command, ICommand
{
    private UpdateYieldCommandDef Params;

    public UpdateYieldCommand(UpdateYieldCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}