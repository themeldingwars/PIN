using GameServer.Enums;
using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Register;

/// <summary>
///     <c>apt::LoadRegisterFromNamedVarCommandDef</c> (type 240): loads the value of a named variable into the
///     aptitude register, combined with the current register through the row's <c>regop</c>.
///
///     GameServer does not keep a named-variable store yet (<c>NamedVariableAssign</c>, type 239, is still a
///     placeholder), so on this server the variable a row asks for is never declared and every execution takes
///     the row's <c>undecl_value</c> fallback. That is what the shared glider pad launch ability needs (chain
///     1001671, row 1001663, variable "WingFX", fallback 1.0): the register comparisons in the launch effects
///     that follow select the pad's effect level through it. The command used to be a placeholder that left the
///     register untouched, so an unset register and a real level were indistinguishable — the chains always
///     took their fallback branch and a row that meant to select its level could never select it.
/// </summary>
public class LoadRegisterFromNamedVarCommand : Command, ICommand
{
    private LoadRegisterFromNamedVarCommandDef Params;

    public LoadRegisterFromNamedVarCommand(LoadRegisterFromNamedVarCommandDef par)
    : base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        // No variable has ever been declared on the server, so the row's fallback is the value to load.
        // A real lookup would resolve Params.NameId/MemberName against the declaring entity's variable
        // store (VarSrctype says which side of the activation owns it) and only fall back when it is
        // truly absent; until assignments exist the two are indistinguishable.
        context.Register = AbilitySystem.RegistryOp(context.Register, Params.UndeclValue, (Operand)Params.Regop);

        return true;
    }
}
