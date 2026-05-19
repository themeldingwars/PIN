using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Register;

public class RegisterLoadFromWeaponCommand : Command, ICommand
{
    private RegisterLoadFromWeaponCommandDef Params;

    public RegisterLoadFromWeaponCommand(RegisterLoadFromWeaponCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }

    public override void Reset(Context context)
    {
    }
}