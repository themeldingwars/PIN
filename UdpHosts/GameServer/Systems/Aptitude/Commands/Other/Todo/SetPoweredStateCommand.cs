using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SetPoweredStateCommand : Command, ICommand
{
    private SetPoweredStateCommandDef Params;

    public SetPoweredStateCommand(SetPoweredStateCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}