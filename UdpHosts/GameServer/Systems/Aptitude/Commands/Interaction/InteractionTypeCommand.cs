using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities;

namespace GameServer.Aptitude;

public class InteractionTypeCommand : Command, ICommand
{
    private InteractionTypeCommandDef Params;

    public InteractionTypeCommand(InteractionTypeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        if (context.Targets.Count > 0)
        {
            var interactionEntity = context.Targets.Peek();
            var hack = interactionEntity as BaseEntity;
            var type = hack.Interaction.Type;

            Logger.Debug("{Command} {CommandId} Compared {type} with {ParamsType}", nameof(InteractionTypeCommand), Params.Id, type, Params.Type);
            return (byte)type == Params.Type;
        }
        else
        {
            return false;
        }
    }
}