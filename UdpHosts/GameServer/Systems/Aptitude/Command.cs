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
    /// <param name="context"></param>
    /// <param name="result"></param>
    public abstract void Execute(Context context, ref CommandResult result);

    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    /// <param name="result"></param>
    public virtual void Func1(Context context, ref CommandResult result)
    {
        result.SetPass(StatusCode.None);
    }

    /// <summary>
    /// Triggered by mResetFlags
    /// </summary>
    /// <param name="context"></param>
    // public abstract void Reset(Context context);
    public virtual void Reset(Context context)
    {
        return;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    public virtual void Func3(Context context)
    {
        return;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    public virtual void Slot(Context context)
    {
        return;
    }
}