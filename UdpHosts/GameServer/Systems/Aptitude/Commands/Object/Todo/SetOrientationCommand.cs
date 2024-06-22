using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SetOrientationCommand : ICommand
{
    private SetOrientationCommandDef Params;

    public SetOrientationCommand(SetOrientationCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}