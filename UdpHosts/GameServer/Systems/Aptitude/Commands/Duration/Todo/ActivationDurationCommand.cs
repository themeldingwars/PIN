using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Duration;

public class ActivationDurationCommand : Command, ICommand
{
    private ActivationDurationCommandDef Params;

    public ActivationDurationCommand(ActivationDurationCommandDef par)
: base(par)
    {
        Params = par;
    }

    // With Activated 1 passes while the player holds the ability's key, with Activated 0 once they let go
    public override void Execute(Context context, ref CommandResult result)
    {
        // Only players report when they let go of an ability
        if (context.Initiator is not CharacterEntity { IsPlayerControlled: true } character)
        {
            result.SetPass();
            return;
        }

        // ActivateAbility stores an AbilityInput on the character, DeactivateAbility removes it when the key is released
        var isHeld = character.TryGetAbilityInput(Params.AbilityId, out _);
        if (isHeld == (Params.Activated == 1))
        {
            result.SetPass();
        }
        else
        {
            result.SetFail(StatusCode.Status5_DurationFail);
        }
    }
}
