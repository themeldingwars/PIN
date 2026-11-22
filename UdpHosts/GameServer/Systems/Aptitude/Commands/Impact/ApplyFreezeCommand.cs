using AeroMessages.GSS.Character.Event;
using GameServer.Entities.Character;
using GameServer.Protocol;
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
            context.CommandState[Id] = null;
        }

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
        if (!context.CommandState.Remove(Id))
        {
            return;
        }

        if (context.Self is CharacterEntity { IsPlayerControlled: true } character)
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