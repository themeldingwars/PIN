namespace GameServer.Aptitude;

public interface ICommand
{
    public uint Id { get; set; }
    public bool Execute(Context context);
    public virtual void OnApply(Context context, ICommandActiveContext activeCommandContext)
    {
    }

    public virtual void OnRemove(Context context, ICommandActiveContext activeCommandContext)
    {
    }
}