using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class UiNamedVariableCommand : ICommand
{
    private UiNamedVariableCommandDef Params;

    public UiNamedVariableCommand(UiNamedVariableCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}