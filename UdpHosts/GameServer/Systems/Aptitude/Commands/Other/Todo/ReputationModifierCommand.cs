using GameServer.Data.SDB.Records.customdata;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class ReputationModifierCommand : Command, ICommand
{
    private ReputationModifierCommandDef Params;

    public ReputationModifierCommand(ReputationModifierCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        foreach (var target in context.Targets)
        {
            if (target is not CharacterEntity character)
            {
                continue;
            }

            // character.Character_BaseController.ReputationBoostModifierProp
            // character.Character_BaseController.ReputationPermanentModifierProp
            // character.Character_BaseController.ReputationZoneModifierProp
            // character.Character_BaseController.ReputationVipModifierProp
            // character.Character_BaseController.ReputationEventModifierProp
        }

        return true;
    }
}