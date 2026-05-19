using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Register;

public class LoadRegisterFromNamedVarCommand : Command, ICommand
{
    private LoadRegisterFromNamedVarCommandDef Params;

    public LoadRegisterFromNamedVarCommand(LoadRegisterFromNamedVarCommandDef par)
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