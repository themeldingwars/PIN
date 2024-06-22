using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SetLookAtTargetCommand : ICommand
{
    private SetLookAtTargetCommandDef Params;

    public SetLookAtTargetCommand(SetLookAtTargetCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // based on ff1189 should act on context.Self
        return true;
    }
}