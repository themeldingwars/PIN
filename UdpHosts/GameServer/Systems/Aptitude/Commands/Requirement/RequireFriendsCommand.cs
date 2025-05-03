using System;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class RequireFriendsCommand : Command, ICommand
{
    private RequireFriendsCommandDef Params;

    public RequireFriendsCommand(RequireFriendsCommandDef par)
        : base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        bool result = false;

        // NOTE: Investigate target handling
        var target = context.Self;

        if (target is CharacterEntity { IsPlayerControlled: true } character)
        {
            result = character.Character_BaseController.FriendCountProp >= Params.Friends;
        }
        else
        {
            Console.WriteLine("RequireFriendsCommand fails because target is not a Character. If this is happening, we should investigate why.");
        }

        return result;
    }
}