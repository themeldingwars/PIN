using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class UpdateWaitCommand : Command, ICommand
{
    private UpdateWaitCommandDef Params;

    public UpdateWaitCommand(UpdateWaitCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}