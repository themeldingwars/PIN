using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SetInteractionTypeCommand : ICommand
{
    private SetInteractionTypeCommandDef Params;

    public SetInteractionTypeCommand(SetInteractionTypeCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}