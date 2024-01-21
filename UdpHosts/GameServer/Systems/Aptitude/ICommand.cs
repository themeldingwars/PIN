namespace GameServer.Aptitude;

public interface ICommand
{
    public bool Execute(Context context);
}