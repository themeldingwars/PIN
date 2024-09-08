using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

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