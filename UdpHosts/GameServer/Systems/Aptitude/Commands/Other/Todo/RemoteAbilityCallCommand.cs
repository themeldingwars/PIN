using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class RemoteAbilityCallCommand : Command, ICommand
{
    private RemoteAbilityCallCommandDef Params;

    public RemoteAbilityCallCommand(RemoteAbilityCallCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}