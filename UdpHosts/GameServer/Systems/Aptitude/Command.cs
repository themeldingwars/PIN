using GameServer.StaticDB.Records;
using Serilog;

namespace GameServer.Systems.Aptitude;

public abstract class Command : ICommand
{
    protected Command(ICommandDef par)
    {
        Id = par.Id;
        Logger = Log.ForContext(GetType());
    }

    public uint Id { get; }
    protected ILogger Logger { get; }

    /// <summary>
    /// The standard execution logic
    /// </summary>
    /// <param name="context">apt::Context</param>
    /// <param name="result">Result</param>
    public abstract void Execute(Context context, ref CommandResult result);

    /// <summary>
    /// Used frequently but unsure why
    /// </summary>
    /// <param name="context">apt::Context</param>
    /// <param name="result">Result</param>
    public virtual void Test(Context context, ref CommandResult result)
    {
        result.SetPass(StatusCode.None);
    }

    /// <summary>
    /// Triggered by mResetFlags
    /// </summary>
    /// <param name="context">apt::Context</param>
    // public abstract void Reset(Context context);
    public virtual void Reset(Context context)
    {
        return;
    }

    /// <summary>
    /// TODO: Add data, std::any Data seems related to cooldown info
    /// </summary>
    /// <param name="context">apt::Context</param>
    public virtual void Cooldown(Context context)
    {
        return;
    }

    /// <summary>
    /// When the ability is slotted, seems to be used to preload and cache stuff
    /// </summary>
    /// <param name="context">apt::Context</param>
    public virtual void Slot(Context context)
    {
        return;
    }
}