using AeroMessages.GSS.V66;

namespace GameServer.Aptitude;

public abstract class BaseActive
{
    public abstract void OnApply(Context context); 
    public abstract void OnRemove(Context context);
}