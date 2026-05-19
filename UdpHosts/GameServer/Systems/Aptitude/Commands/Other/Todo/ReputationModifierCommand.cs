using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class ReputationModifierCommand : Command, ICommand
{
    private ReputationModifierCommandDef Params;

    public ReputationModifierCommand(ReputationModifierCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
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

        result.SetPass();
        return;
    }
}