using System;

namespace GameServer.Systems.Aptitude;

public interface ICommand
{
    public uint Id { get; }

    public void Execute(Context context, ref CommandResult result);

    public virtual void Test(Context context, ref CommandResult result)
    {
        result.SetPass(StatusCode.None);
    }

    public virtual void Reset(Context context)
    {
        return;
    }

    public virtual void Cooldown(Context context)
    {
        return;
    }

    public virtual void Slot(Context context)
    {
        return;
    }
}