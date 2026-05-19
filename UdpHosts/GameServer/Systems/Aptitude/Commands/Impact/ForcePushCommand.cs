using System.Numerics;
using AeroMessages.GSS.Character.Event;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Impact;

public class ForcePushCommand : Command, ICommand
{
    private ForcePushCommandDef Params;

    public ForcePushCommand(ForcePushCommandDef par)
: base(par)
    {
        Params = par;
    }

    // TODO: Implement based on params, currently hardcoded
    public override void Execute(Context context, ref CommandResult result)
    {
        foreach (IAptitudeTarget target in context.Targets)
        {
            if (target is CharacterEntity character)
            {
                if (!character.IsPlayerControlled)
                {
                    continue;
                }

                var velocity = new Vector3(character.Velocity[0], character.Velocity[1], character.Velocity[2]);
                velocity.Z += 45;

                var player = character.Player;
                var message = new ForcedMovement
                {
                    Data = new AeroMessages.GSS.ForcedMovementData
                    {
                        Type = 5,
                        Params5 = new AeroMessages.GSS.ForcedMovementType5Params
                        {
                            Velocity = velocity,
                            Time1 = context.Shard.CurrentTime + 19,
                            Time2 = context.Shard.CurrentTime + 20,
                            Unk2 = 0
                        }
                    },

                    ShortTime = context.Shard.CurrentShortTime,
                };
                player.NetChannels[ChannelType.ReliableGss].SendMessage(message, character.EntityId);
            }
        }

        result.SetPass();
        return;
    }
}