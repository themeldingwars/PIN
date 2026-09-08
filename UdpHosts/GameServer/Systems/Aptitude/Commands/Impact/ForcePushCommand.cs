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
    // launch pads of the original game. StrengthRegop 1 folds the chain register into the strength additively
    // (the glider pad chain zeroes the register and then adds +3 per special pad module / +10 for boost
    // effects before reaching ForcePush), so ADD can only make the launch stronger, never weaker; an
    // unset register (NaN) is treated as zero. Loft and impact_position affect direction and impact details
    // that are not read yet either.
    public bool Execute(Context context)
    {
        float strength = Params.Strength;
        if (Params.StrengthRegop == 1 && !float.IsNaN(context.Register))
        {
            strength += context.Register;
        }
        var targets = new List<IAptitudeTarget>(context.Targets);
        // If the ability applies to the deployable itself (e.g. glider ability), push the deployable's owner.
        if (context.Self is CharacterEntity || (context.Self as DeployableEntity)?.Owner != null)
        {
            targets.Add(context.Self);
        }

        var visited = new HashSet<ulong>();
        foreach (IAptitudeTarget target in targets)
        {
            var id = target != null ? target.AeroEntityId.Backing : 0;
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

                // The client holds the forced movement (and its velocity) for the [Time1, Time2] window on the
                // shared epoch clock (time-synced against Shard.CurrentTime). A window must be real for the
                // client to apply anything at all: a 1 ms window expires before, or while, the packet is in
                // flight, which is exactly why effects like the glider pad launch played their animation but
                // never moved the player. Start 50 ms out so the push survives latency/jitter and keep it for
                // 500 ms, matching the launch effect's own 500 ms restrict_movement duration.
                var player = character.Player;
                var message = new ForcedMovement
                {
                    Data = new AeroMessages.GSS.ForcedMovementData
                    {
                        Type = 5,
                        HaveUnk2 = 0,
                        Params5 = new AeroMessages.GSS.ForcedMovementType5Params
                        {
                            Velocity = velocity,
                            Time1 = context.Shard.CurrentTime + 50,
                            Time2 = context.Shard.CurrentTime + 550,
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