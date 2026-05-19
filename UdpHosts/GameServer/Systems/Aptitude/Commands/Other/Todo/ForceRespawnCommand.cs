using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class ForceRespawnCommand : Command, ICommand
{
    private ForceRespawnCommandDef Params;

    public ForceRespawnCommand(ForceRespawnCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}