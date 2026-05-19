using AeroMessages.GSS.Character.Event;
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

    public override void Execute(Context context, ref CommandResult result)
    {
        var target = context.Self;

        if (target is CharacterEntity)
        {
            context.Actives.Add(this, null);
        }

        result.SetPass();
        return;
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