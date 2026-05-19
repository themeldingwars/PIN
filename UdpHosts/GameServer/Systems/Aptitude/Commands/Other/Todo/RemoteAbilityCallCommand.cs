using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

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