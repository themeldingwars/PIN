using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SetYawCommand : Command, ICommand
{
    private SetYawCommandDef Params;

    public SetYawCommand(SetYawCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}