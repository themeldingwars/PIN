using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SetGuardianCommand : ICommand
{
    private SetGuardianCommandDef Params;

    public SetGuardianCommand(SetGuardianCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}