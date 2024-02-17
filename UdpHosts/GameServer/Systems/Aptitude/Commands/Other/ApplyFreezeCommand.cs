using System;
using System.Collections.Generic;
using System.Numerics;
using AeroMessages.GSS.V66.Character.Event;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;
using GameServer.Enums;

namespace GameServer.Aptitude;

public class ApplyFreezeCommand : ICommand
{
    private ApplyFreezeCommandDef Params;

    public ApplyFreezeCommand(ApplyFreezeCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var target = context.Self;

        if (target.GetType() == typeof(Entities.Character.CharacterEntity))
        {
            context.Actives.Add(this, null);
        }

        return true;
    }

    public void OnRemove(Context context, ICommandActiveContext activeCommandContext)
    {
        var target = context.Self;
        if (target.GetType() == typeof(Entities.Character.CharacterEntity))
        {
            var character = target as Entities.Character.CharacterEntity;
            if (character.IsPlayerControlled)
            {
                Console.WriteLine("ApplyFreezeCommand Sending ForcedMovementCancelled");
                var player = character.Player;
                var message = new ForcedMovementCancelled
                {
                    CommandId = Params.Id,
                    ShortTime = context.Shard.CurrentShortTime,
                };
                player.NetChannels[ChannelType.ReliableGss].SendIAero(message, character.EntityId);
            }
        }
    }
    
}