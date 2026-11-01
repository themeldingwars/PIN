using GameServer.Enums;
using GameServer.StaticDB.Records.apt;
using GameServer.Systems.Aptitude.NamedVars;

namespace GameServer.Systems.Aptitude.Commands.Register;

public class LoadRegisterFromNamedVarCommand : LoadRegisterBaseCommand, ICommand
{
    private LoadRegisterFromNamedVarCommandDef Params;
    private NamedVarKey Key;

    public LoadRegisterFromNamedVarCommand(LoadRegisterFromNamedVarCommandDef par)
: base(par)
    {
        Params = par;
        Key = NamedVarKey.FromDef(par.NameId, par.MemberName);
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        var value = Params.UndeclValue;
        if (context.TryGetNamedVarStore(Params.VarSrctype, allowGlobal: true, out var store)
            && store.TryGet(Key, out var stored))
        {
            value = stored;
        }

        context.Register = AbilitySystem.RegistryOp(context.Register, value, (Operand)Params.Regop);

        Logger.Debug("{Command} {CommandId}: {Key} => {value} (undecl {UndeclValue}, srctype {Srctype}, register {Register})",
            nameof(LoadRegisterFromNamedVarCommand),
            Params.Id,
            NamedVarKey.Describe(Key, Params.MemberName),
            value,
            Params.UndeclValue,
            Params.VarSrctype,
            context.Register);

        result.SetPass();
        return;
    }

    public override void Reset(Context context)
    {
    }
}
