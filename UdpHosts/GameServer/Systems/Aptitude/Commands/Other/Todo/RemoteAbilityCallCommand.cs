using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class RemoteAbilityCallCommand : ICommand
{
    private RemoteAbilityCallCommandDef Params;

    public RemoteAbilityCallCommand(RemoteAbilityCallCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}