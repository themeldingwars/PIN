using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class UnlockBattleframesCommand : ICommand
{
    private UnlockBattleframesCommandDef Params;

    public UnlockBattleframesCommand(UnlockBattleframesCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}