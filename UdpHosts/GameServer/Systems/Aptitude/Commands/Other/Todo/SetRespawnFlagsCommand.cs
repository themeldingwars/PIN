using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SetRespawnFlagsCommand : ICommand
{
    private SetRespawnFlagsCommandDef Params;

    public SetRespawnFlagsCommand(SetRespawnFlagsCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}