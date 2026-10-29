using GameServer.Enums;
using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class NamedVariableAssignCommand : Command, ICommand
{
    private NamedVariableAssignCommandDef Params;

    public NamedVariableAssignCommand(NamedVariableAssignCommandDef par)
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

        var current = owner.NamedVariables.TryGetValue(Params.NameId, out var value) ? value : float.NaN;
        owner.NamedVariables[Params.NameId] = AbilitySystem.RegistryOp(current, Params.Value, (Operand)Params.Regop);
        result.SetPass();
    }
}