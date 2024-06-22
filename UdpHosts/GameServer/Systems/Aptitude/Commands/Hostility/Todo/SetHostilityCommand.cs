using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SetHostilityCommand : ICommand
{
    private SetHostilityCommandDef Params;

    public SetHostilityCommand(SetHostilityCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}