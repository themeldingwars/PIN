using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Target;

public class TargetMyTinyObjectsCommand : Command, ICommand
{
    private TargetMyTinyObjectsCommandDef Params;

    public TargetMyTinyObjectsCommand(TargetMyTinyObjectsCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }

    public override void Reset(Context context)
    {
        return;
    }
}