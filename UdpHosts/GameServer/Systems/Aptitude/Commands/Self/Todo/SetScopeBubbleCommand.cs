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

    public bool Execute(Context context)
    {
        // based on ff1189 should act on context.Self
        return true;
    }
}