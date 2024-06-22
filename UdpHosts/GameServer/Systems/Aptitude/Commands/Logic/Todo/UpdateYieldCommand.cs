using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class UpdateYieldCommand : ICommand
{
    private UpdateYieldCommandDef Params;

    public UpdateYieldCommand(UpdateYieldCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}