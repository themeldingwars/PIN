using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Unlock;

public class UnlockBattleframesCommand : Command, ICommand
{
    private UnlockBattleframesCommandDef Params;

    public UnlockBattleframesCommand(UnlockBattleframesCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (Params.SdbId == 0)
        {
            result.SetPass();
            return;
        }

        if (context.Self is CharacterEntity character)
        {
        }

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
        return;
    }
}