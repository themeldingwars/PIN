using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class UpdateWaitCommand : ICommand
{
    private UpdateWaitCommandDef Params;

    public UpdateWaitCommand(UpdateWaitCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}