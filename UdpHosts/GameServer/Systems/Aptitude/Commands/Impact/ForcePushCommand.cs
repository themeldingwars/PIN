using System.Collections.Generic;
using System.Numerics;
using AeroMessages.GSS.Character.Event;
using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
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

    // The row's strength drives the impulse (the glider pad's row carries 30), pushed straight up like the
    // launch pads of the original game. StrengthRegop would fold the chain register into the strength, but
    // the direction of that operation needs client captures to pin down; until then the register must not
    // be allowed to zero the impulse out (the launch effect's own chain resets the register as it goes), so
    // the row's strength is used as-is. Loft and impact_position affect direction and impact details that
    // are not read yet either.
    public bool Execute(Context context)
    {
        float strength = Params.Strength;
        var targets = new List<IAptitudeTarget>(context.Targets);
        // If the ability applies to the deployable itself (e.g. glider ability), push the deployable's owner.
        if (context.Self is CharacterEntity || (context.Self as DeployableEntity)?.Owner != null)
        {
            targets.Add(context.Self);
        }

        var visited = new HashSet<ulong>();
        foreach (IAptitudeTarget target in targets)
        {
            var id = target?.AeroEntityId?.Backing ?? 0;
            if (id == 0 || !visited.Add(id))
            {
                continue;
            }
            var character = target as CharacterEntity ?? (target as DeployableEntity)?.Owner;
            if (character == null)
            {
                continue;
            }

            if (!character.IsPlayerControlled)
            {
                continue;
            }

                var velocity = new Vector3(character.Velocity[0], character.Velocity[1], character.Velocity[2]);
                velocity.Z += strength;

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

        return true;
    }
}