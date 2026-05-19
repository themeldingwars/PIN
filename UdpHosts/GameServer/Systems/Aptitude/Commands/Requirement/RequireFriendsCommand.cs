using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireFriendsCommand : Command, ICommand
{
    private RequireFriendsCommandDef Params;

    public RequireFriendsCommand(RequireFriendsCommandDef par)
        : base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        bool cmdResult = false;

        // NOTE: Investigate target handling
        var target = context.Self;

        if (target is CharacterEntity { IsPlayerControlled: true } character)
        {
            cmdResult = character.Character_BaseController.FriendCountProp >= Params.Friends;
        }
        else
        {
            Logger.Warning("{Command} {CommandId} fails because target is not a Character. If this is happening, we should investigate why.", nameof(RequireFriendsCommand), Params.Id);
        }

        if (cmdResult)
        {
            result.SetPass();
        }
        else
        {
            result.SetFail();
        }

        return;
    }

    public override void Reset(Context context)
    {
        return;
    }
}