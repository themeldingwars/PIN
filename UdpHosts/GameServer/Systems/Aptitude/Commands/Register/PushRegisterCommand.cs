using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class PushRegisterCommand : Command, ICommand
{
    private PushRegisterCommandDef Params;

    public PushRegisterCommand(PushRegisterCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        context.FormerRegister = context.Register;
        context.Register = 0;

        return true;
    }
}
