using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SetRespawnFlagsCommand : Command, ICommand
{
    private SetRespawnFlagsCommandDef Params;

    public SetRespawnFlagsCommand(SetRespawnFlagsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}