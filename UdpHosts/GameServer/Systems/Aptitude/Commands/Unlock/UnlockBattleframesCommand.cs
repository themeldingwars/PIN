using GameServer.Data;
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

        if (context.Self is CharacterEntity { IsPlayerControlled: true } character)
        {
            // Records that the chassis is unlocked. Granting the chassis item and its loadout is still missing.
            character.Player.Unlocks.Grant(UnlockTypes.Battleframe, Params.SdbId);
        }

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
        return;
    }
}