using System.Numerics;
using AeroMessages.GSS;
using AeroMessages.GSS.Character.Event;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Protocol;
using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Movement;

public class TeleportCommand : Command, ICommand
{
    private TeleportCommandDef Params;

    public TeleportCommand(TeleportCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var destination = context.InitPosition;

        if (context.Self is CharacterEntity character)
        {
            character.SetPosition(destination);
            context.Shard.Physics.UpdateEntity(character);

            if (character.Player is { } player)
            {
                var forcedMove = new ForcedMovement
                {
                    Data = new ForcedMovementData
                    {
                        Type = 1,
                        CommandId = 0,
                        HaveRelativeTo = 0,
                        Params1 = new ForcedMovementType1Params { Position = destination, Direction = character.AimDirection, Velocity = Vector3.Zero, Time = context.Shard.CurrentTime + 1 }
                    },
                    ShortTime = context.Shard.CurrentShortTime
                };
                player.NetChannels[ChannelType.ReliableGss].SendMessage(forcedMove, character.EntityId);
            }
        }
        else if (context.Self is BaseEntity entity)
        {
            entity.Position = destination;
            context.Shard.Physics.UpdateEntity(entity);
        }
        else
        {
            result.SetFail(StatusCode.Error);
            return;
        }

        Logger.Debug("{Command} {CommandId} moved {Self} to {Destination}", nameof(TeleportCommand), Params.Id, context.Self, destination);
        result.SetPass();
    }
}