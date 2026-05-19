using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class ReviveCommand : Command, ICommand
{
    private ReviveCommandDef Params;

    public ReviveCommand(ReviveCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // based on ff1189 should act on context.Targets
        return true;
    }
}