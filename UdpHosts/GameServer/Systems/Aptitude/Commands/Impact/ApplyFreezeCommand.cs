using AeroMessages.GSS.V66.Character.Event;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Impact;

public class ApplyFreezeCommand : Command, ICommand
{
    private ApplyFreezeCommandDef Params;

    public ApplyFreezeCommand(ApplyFreezeCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var target = context.Self;

        if (target is CharacterEntity)
        {
            context.Actives.Add(this, null);
        }

        return true;
    }

    public void OnRemove(Context context, ICommandActiveContext activeCommandContext)
    {
        var target = context.Self;
        if (target is CharacterEntity { IsPlayerControlled: true } character)
        {
            Logger.Information("{Command} Sending ForcedMovementCancelled {CommandId}", nameof(ApplyFreezeCommand), Params.Id);
            var player = character.Player;
            var message = new ForcedMovementCancelled
            {
                CommandId = Params.Id,
                ShortTime = context.Shard.CurrentShortTime,
            };
            player.NetChannels[ChannelType.ReliableGss].SendMessage(message, character.EntityId);
        }
    }
}