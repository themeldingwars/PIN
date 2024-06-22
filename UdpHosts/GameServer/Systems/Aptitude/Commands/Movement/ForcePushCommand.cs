using System;
using System.Numerics;
using AeroMessages.GSS.V66.Character.Event;
using GameServer.Data.SDB.Records.aptfs;

namespace GameServer.Aptitude;

public class ForcePushCommand : ICommand
{
    private ForcePushCommandDef Params;

    public ForcePushCommand(ForcePushCommandDef par)
    {
        Params = par;
    }

    // TODO: Implement based on params, currently hardcoded
    public bool Execute(Context context)
    {
        foreach (IAptitudeTarget target in context.Targets)
        {
            if (target.GetType() == typeof(Entities.Character.CharacterEntity))
            {
                var character = target as Entities.Character.CharacterEntity;
                
                if (!character.IsPlayerControlled)
                {
                    continue;
                }

                var velocity = new Vector3(character.Velocity[0], character.Velocity[1], character.Velocity[2]);
                velocity.Z += 45;

                var player = character.Player;
                var message = new ForcedMovement
                {
                    Data = new AeroMessages.GSS.V66.ForcedMovementData
                    {
                        Type = 5,
                        Params5 = new AeroMessages.GSS.V66.ForcedMovementType5Params
                        {
                            Velocity = velocity,
                            Time1 = context.Shard.CurrentTime + 19,
                            Time2 = context.Shard.CurrentTime + 20,
                            Unk2 = 0
                        }
                    },

                    ShortTime = context.Shard.CurrentShortTime,
                };
                player.NetChannels[ChannelType.ReliableGss].SendIAero(message, character.EntityId);
            }
        }

        return true;
    }
}