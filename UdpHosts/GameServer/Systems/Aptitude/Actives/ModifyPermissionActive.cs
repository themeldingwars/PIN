using AeroMessages.GSS.V66;
using GameServer.Data.SDB.Records.customdata;

namespace GameServer.Aptitude;

public class ModifyPermissionActive : BaseActive
{
    private ModifyPermissionCommand Command;

    public ModifyPermissionActive(ModifyPermissionCommand command)
    {
        Command = command;
    }

    public override void OnApply(Context context)
    {
        Command.OnApply(context);
    }

    public override void OnRemove(Context context)
    {
        Command.OnRemove(context);
    }
}