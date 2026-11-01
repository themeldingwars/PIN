using GameServer.Enums;
using GameServer.StaticDB.Records.apt;
using GameServer.Systems.Aptitude.NamedVars;

namespace GameServer.Systems.Aptitude.Commands.Register;

public class NamedVariableAssignCommand : Command, ICommand
{
    private NamedVariableAssignCommandDef Params;
    private NamedVarKey Key;

    public NamedVariableAssignCommand(NamedVariableAssignCommandDef par)
: base(par)
    {
        Params = par;
        Key = NamedVarKey.FromDef(par.NameId, par.MemberName);
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (!context.TryGetNamedVarStore(Params.VarSrctype, allowGlobal: false, out var store) || store.IsReadOnly)
        {
            Logger.Debug("{Command} {CommandId}: {Key} has no writable store for srctype {Srctype}, failing",
                nameof(NamedVariableAssignCommand),
                Params.Id,
                Key,
                Params.VarSrctype);
            result.SetFail();
            return;
        }

        float value = AbilitySystem.RegistryOp(context.Register, Params.Value, (Operand)Params.Regop);
        store.Set(Key, value);

        Logger.Debug("{Command} {CommandId}: {Key} = {value} on {Store} (register {Register}, {Op})",
            nameof(NamedVariableAssignCommand),
            Params.Id,
            NamedVarKey.Describe(Key, Params.MemberName),
            value,
            store,
            context.Register,
            (Operand)Params.Regop);

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
    }
}
