using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Interaction;

public class InteractionInProgressCommand : Command, ICommand
{
    private InteractionInProgressCommandDef Params;

    public InteractionInProgressCommand(InteractionInProgressCommandDef par)
: base(par)
    {
        Params = par;
    }

    // Passes until the interaction has taken as long as its target asks for
    public override void Execute(Context context, ref CommandResult result)
    {
        if (context.Self is CharacterEntity { InteractionTarget: BaseEntity { Interaction: { } interaction } } character
            && context.Shard.CurrentTimeLong - character.InteractionStartedAt < interaction.DurationMs)
        {
            // Wait with a pass (raw 0x00060003, the value HasTargetsDuration knows as wait_pass_status6):
            // the chain stops here and the effect stays
            result.SetPass(StatusCode.Status6);
            result.Yield = true;
            return;
        }

        result.SetFail(StatusCode.Status5_DurationFail);
    }
}