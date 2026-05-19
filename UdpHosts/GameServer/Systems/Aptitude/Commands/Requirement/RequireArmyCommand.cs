using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireArmyCommand : Command, ICommand
{
    private RequireArmyCommandDef Params;

    public RequireArmyCommand(RequireArmyCommandDef par)
        : base(par)
    {
        Params = par;
    }

    /*
     * Params.CheckTarget and Params.Rank are equal to 0 for all 3 instances
     * Possibly Rank = webapi's ArmyRank.Position
     */
    public bool Execute(Context context)
    {
        if (Params.CheckTarget == 1)
        {
            if (context.Targets.TryPeek(out var t) && t is CharacterEntity target)
            {
                return target.Character_BaseController.ArmyGUIDProp != 0;
            }
        }
        else if (context.Self is CharacterEntity self)
        {
            return self.Character_BaseController.ArmyGUIDProp != 0;
        }

        return false;
    }
}