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

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}