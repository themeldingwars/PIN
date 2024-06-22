using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SetPoweredStateCommand : ICommand
{
    private SetPoweredStateCommandDef Params;

    public SetPoweredStateCommand(SetPoweredStateCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}