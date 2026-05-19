using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Duration;

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