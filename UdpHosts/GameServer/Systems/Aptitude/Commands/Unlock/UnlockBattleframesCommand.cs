using GameServer.Data.SDB.Records.customdata;
using GameServer.Entities.Character;

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
        if (Params.SdbId == 0)
        {
            return true;
        }

        if (context.Self is CharacterEntity character)
        {
        }

        return true;
    }
}