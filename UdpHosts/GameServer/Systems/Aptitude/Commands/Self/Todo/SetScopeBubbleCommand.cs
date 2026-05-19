using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Self;

public class SetScopeBubbleCommand : Command, ICommand
{
    private SetScopeBubbleCommandDef Params;

    public SetScopeBubbleCommand(SetScopeBubbleCommandDef par)
: base(par)
    {
        Params = par;
    }

    // based on ff1189 should act on context.Self
    public override void Execute(Context context, ref CommandResult result)
    {
    }

    public override void Reset(Context context)
    {
        return;
    }
}