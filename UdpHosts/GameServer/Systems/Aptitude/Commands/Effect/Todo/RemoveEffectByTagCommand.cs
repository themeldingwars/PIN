using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class RemoveEffectByTagCommand : ICommand
{
    private RemoveEffectByTagCommandDef Params;

    public RemoveEffectByTagCommand(RemoveEffectByTagCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}