using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class BattleFrameDurationCommand : Command, ICommand
{
    private BattleFrameDurationCommandDef Params;

    public BattleFrameDurationCommand(BattleFrameDurationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}