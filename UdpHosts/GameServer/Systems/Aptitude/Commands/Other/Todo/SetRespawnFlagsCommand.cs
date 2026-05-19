using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

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