using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Register;

public class PopRegisterCommand : Command, ICommand
{
    private PopRegisterCommandDef Params;

    public PopRegisterCommand(PopRegisterCommandDef par)
: base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        context.Register = context.FormerRegister;
        context.FormerRegister = 0;

        return true;
    }
}