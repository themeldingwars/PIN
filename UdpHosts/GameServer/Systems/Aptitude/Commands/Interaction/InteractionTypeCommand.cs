using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Interaction;

public class InteractionTypeCommand : Command, ICommand
{
    private InteractionTypeCommandDef Params;

    public InteractionTypeCommand(InteractionTypeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (context.Self is not CharacterEntity character || character.InteractionTarget == null)
        {
            Logger.Warning("{Command} {CommandId} Called with bad state. Self {Self}, InteractionTarget {InteractionTarget}",  nameof(EndInteractionCommand), Id, context.Self, ((CharacterEntity)context.Self).InteractionTarget);
            result.SetFail(StatusCode.PINError);
            return;
        }

        var source = context.Self as CharacterEntity;
        var target = (BaseEntity)source.InteractionTarget;
        var targetType = (InteractionType)target.GetInteractionType();
        var paramsType = (InteractionType)Params.Type;

        Logger.Debug("{Command} {CommandId} Compared {TargetType} with {ParamsType}", nameof(InteractionTypeCommand), Params.Id, targetType, paramsType);

        if (targetType == paramsType)
        {
            result.SetPass(StatusCode.None);
        }
        else
        {
            result.SetFail(StatusCode.Status1);
        }

        return;
    }
}