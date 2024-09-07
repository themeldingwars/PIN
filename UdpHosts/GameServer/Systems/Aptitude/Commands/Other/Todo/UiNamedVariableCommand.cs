using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class UiNamedVariableCommand : Command, ICommand
{
    private UiNamedVariableCommandDef Params;

    public UiNamedVariableCommand(UiNamedVariableCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}