using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class RemoveClientStatusEffectCommand : ICommand
{
    private RemoveClientStatusEffectCommandDef Params;

    public RemoveClientStatusEffectCommand(RemoveClientStatusEffectCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}