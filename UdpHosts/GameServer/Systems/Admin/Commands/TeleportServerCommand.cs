using System.Numerics;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.Character.Event;

namespace GameServer.Admin;

[ServerCommand("Teleport your character", "tp <x> <y> <z>", "tp", "teleport")]
public class TeleportServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        if (parameters.Length != 3)
        {
            SourceFeedback("Invalid number of parameters for teleport command", context);
            return;
        }

        if (context.SourcePlayer == null || context.SourcePlayer.CharacterEntity == null)
        {
            SourceFeedback("Cannot teleport without a valid player character", context);
            return;
        }

        var character = context.SourcePlayer.CharacterEntity;

        if (TryParseFloat(parameters[0], out float x) &&
            TryParseFloat(parameters[1], out float y) &&
            TryParseFloat(parameters[2], out float z))
        {
            Vector3 destination = new Vector3(x, y, z);
            
            character.SetPosition(destination);
            var forcedMove = new ForcedMovement
            {
                Data = new ForcedMovementData
                {
                    Type = 1,
                    Unk1 = 0,
                    HaveUnk2 = 0,
                    Params1 = new ForcedMovementType1Params { Position = destination, Direction = character.AimDirection, Velocity = Vector3.Zero, Time = character.Shard.CurrentTime + 1 }
                },
                ShortTime = character.Shard.CurrentShortTime
            };
            context.SourcePlayer.NetChannels[ChannelType.ReliableGss].SendMessage(forcedMove, character.EntityId);
        }
        else
        {
           SourceFeedback("Invalid float format in teleport command", context);
        }
    }

    private bool TryParseFloat(string input, out float result)
    {
        return float.TryParse(input, out result);
    }
}