namespace GameServer.Aptitude;

public interface ICommand
{
    public bool Execute(Context context);
    public virtual void OnApply(Context context)
    {
    }

    public virtual void OnRemove(Context context)
    {
    }
}