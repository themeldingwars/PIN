using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SetOrientationCommand : Command, ICommand
{
    private SetOrientationCommandDef Params;

    public SetOrientationCommand(SetOrientationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}