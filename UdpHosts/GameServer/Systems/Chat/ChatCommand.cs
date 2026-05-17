using System.Numerics;
using Serilog;

namespace GameServer.Systems.Chat;

public abstract class ChatCommand
{
    protected readonly ILogger _logger;

    protected ChatCommand()
    {
        _logger = Log.ForContext(GetType());
    }

    public abstract void Execute(string[] parameters, ChatCommandContext context);
    public virtual void SourceFeedback(string message, ChatCommandContext context)
    {
        _logger.Information(message);
        context.SourcePlayer?.SendDebugChat(message);
    }

    public uint ParseUIntParameter(string value)
    {
        if (uint.TryParse(value, out uint result))
        {
            return result;
        }

        _logger.Warning("Invalid format: {value}", value);
        return 0;
    }

    public ulong ParseULongParameter(string value)
    {
        if (ulong.TryParse(value, out ulong result))
        {
            return result;
        }

        _logger.Warning("Invalid format: {value}", value);
        return 0;
    }

    public Vector3? ParseVector3Parameters(string[] parameters, int startIndex = 0)
    {
        if (startIndex < 0 || startIndex >= parameters.Length)
        {
            _logger.Warning("Invalid start index: {startIndex}", startIndex);
            return null;
        }

        if (startIndex + 2 < parameters.Length &&
            float.TryParse(parameters[startIndex], out float x) &&
            float.TryParse(parameters[startIndex + 1], out float y) &&
            float.TryParse(parameters[startIndex + 2], out float z))
        {
            return new Vector3(x, y, z);
        }

        _logger.Warning("Invalid format for Vector3 parameters");
        return null;
    }
}