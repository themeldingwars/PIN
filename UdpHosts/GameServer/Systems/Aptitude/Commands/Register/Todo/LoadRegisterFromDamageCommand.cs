using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Register;

public class LoadRegisterFromDamageCommand : Command, ICommand
{
    private LoadRegisterFromDamageCommandDef Params;

    public LoadRegisterFromDamageCommand(LoadRegisterFromDamageCommandDef par)
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