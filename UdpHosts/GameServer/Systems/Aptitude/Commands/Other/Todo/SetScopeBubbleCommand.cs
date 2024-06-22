using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SetScopeBubbleCommand : ICommand
{
    private SetScopeBubbleCommandDef Params;

    public SetScopeBubbleCommand(SetScopeBubbleCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // based on ff1189 should act on context.Self
        return true;
    }
}