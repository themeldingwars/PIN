using System;

namespace GameServer.Systems.Aptitude;

public interface ICommand
{
    public uint Id { get; }

    public void Execute(Context context, ref CommandResult result);

    public virtual void Func1(Context context, ref CommandResult result)
    {
        result.SetPass(StatusCode.None);
    }

    public virtual void Reset(Context context)
    {
        return;
    }

    public virtual void Func3(Context context)
    {
        return;
    }

    public virtual void Slot(Context context)
    {
        return;
    }

    [Obsolete("To be replaced with more gamelike behavior")]
    public virtual void OnApply(Context context, ICommandActiveContext activeCommandContext)
    {
    }

    [Obsolete("To be replaced with more gamelike behavior")]
    public virtual void OnRemove(Context context, ICommandActiveContext activeCommandContext)
    {
    }
}