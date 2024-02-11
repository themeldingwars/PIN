namespace GameServer.Aptitude;

public interface ICommand
{
    public bool Execute(Context context);
    public virtual void OnApply(Context context, ICommandActiveContext activeCommandContext)
    {
    }

    public virtual void OnRemove(Context context, ICommandActiveContext activeCommandContext)
    {
    }
}