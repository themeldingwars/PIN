using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class ApplyClientStatusEffectCommand : ICommand
{
    private ApplyClientStatusEffectCommandDef Params;

    public ApplyClientStatusEffectCommand(ApplyClientStatusEffectCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}