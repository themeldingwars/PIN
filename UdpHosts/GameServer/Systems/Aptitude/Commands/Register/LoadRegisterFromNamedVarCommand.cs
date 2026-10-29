using GameServer.Enums;
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
        if (NamedVariableOwner.Of(context, Params.VarSrctype) is not { } owner)
        {
            result.SetFail();
            return;
        }

        var value = owner.NamedVariables.TryGetValue(Params.NameId, out var assigned) ? assigned : Params.UndeclValue;
        context.Register = AbilitySystem.RegistryOp(context.Register, value, (Operand)Params.Regop);
        result.SetPass();
    }

    public override void Reset(Context context)
    {
    }
}