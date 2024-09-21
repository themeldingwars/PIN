using System;
using System.Numerics;
using GameServer.Entities;
using GameServer.Entities.Character;

namespace GameServer.Admin;

public abstract class ServerCommand
{
    public abstract void Execute(string[] parameters, ServerCommandContext context);
    public virtual void SourceFeedback(string message, ServerCommandContext context)
    {
        Console.WriteLine(message);
        context.SourcePlayer?.SendDebugChat(message);
    }

    public IEntity GetGunTarget(CharacterEntity character, ServerCommandContext context)
    {
        var direction = character.AimDirection;
        var origin = character.GetProjectileOrigin(direction);
        (bool hit, Vector3 loc, ulong entId) = context.Shard.Physics.TargetRayCast(origin, direction, character);
        if (hit)
        {
            return context.Shard.Entities.TryGetValue(entId, out var value) ? value : null;
        }

        return null;
    }

    public uint ParseUIntParameter(string value)
    {
        if (uint.TryParse(value, out uint result))
        {
            return result;
        }
        else
        {
            Console.WriteLine($"Invalid format: {value}");
            return 0;
        }
    }

    public ulong ParseULongParameter(string value)
    {
        if (ulong.TryParse(value, out ulong result))
        {
            return result;
        }
        else
        {
            Console.WriteLine($"Invalid format: {value}");
            return 0;
        }
    }

    public Vector3? ParseVector3Parameters(string[] parameters, int startIndex = 0)
    {
        if (startIndex < 0 || startIndex >= parameters.Length)
        {
            Console.WriteLine($"Invalid start index: {startIndex}");
            return null;
        }

        if (startIndex + 2 < parameters.Length &&
            float.TryParse(parameters[startIndex], out float x) &&
            float.TryParse(parameters[startIndex + 1], out float y) &&
            float.TryParse(parameters[startIndex + 2], out float z))
        {
            return new Vector3(x, y, z);
        }
        else
        {
            Console.WriteLine("Invalid format for Vector3 parameters");
            return null;
        }
    }
}