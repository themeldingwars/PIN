using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class SetHoverParametersCommand : ICommand
{
    private SetHoverParametersCommandDef Params;

    public SetHoverParametersCommand(SetHoverParametersCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}