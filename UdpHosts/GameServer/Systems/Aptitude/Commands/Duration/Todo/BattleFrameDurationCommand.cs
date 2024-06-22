using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class BattleFrameDurationCommand : ICommand
{
    private BattleFrameDurationCommandDef Params;

    public BattleFrameDurationCommand(BattleFrameDurationCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        return true;
    }
}