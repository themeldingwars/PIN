using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class RequireIsNPCCommand : Command, ICommand
{
    private RequireIsNPCCommandDef Params;

    public RequireIsNPCCommand(RequireIsNPCCommandDef par)
        : base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        bool result = false;

        // NOTE: Investigate target handling
        var target = context.Self;

        if (target is CharacterEntity character)
        {
            result = character.IsPlayerControlled;
        }
        else
        {
            Logger.Warning("{Command} {CommandId} fails because target is not a Character. If this is happening, we should investigate why.", nameof(RequireIsNPCCommand), Params.Id);
        }

        if (Params.Negate == 1)
        {
            result = !result;
        }

        return result;
    }
}