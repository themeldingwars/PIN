using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class UnlockBattleframesCommand : Command, ICommand
{
    private UnlockBattleframesCommandDef Params;

    public UnlockBattleframesCommand(UnlockBattleframesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}