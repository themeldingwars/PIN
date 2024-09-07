using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SetInteractionTypeCommand : Command, ICommand
{
    private SetInteractionTypeCommandDef Params;

    public SetInteractionTypeCommand(SetInteractionTypeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}