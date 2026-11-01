using GameServer.StaticDB.Records.aptfs;
using GameServer.Systems.Aptitude.NamedVars;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class UiNamedVariableCommand : Command, ICommand
{
    private UiNamedVariableCommandDef Params;
    private NamedVarKey Key;

    public UiNamedVariableCommand(UiNamedVariableCommandDef par)
: base(par)
    {
        Params = par;
        Key = NamedVarKey.FromDef(par.NameId, par.MemberName);
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (TryGetWatchedStore(context, out var store))
        {
            store.Watch(Key);
            Logger.Debug("{Command} {CommandId}: watching {Key} as {UiName} on {Store}",
                nameof(UiNamedVariableCommand),
                Params.Id,
                NamedVarKey.Describe(Key, Params.MemberName),
                Params.UiName,
                store);
        }
        else
        {
            Logger.Debug("{Command} {CommandId}: {Key} has no store for srctype {Srctype}, nothing to watch",
                nameof(UiNamedVariableCommand),
                Params.Id,
                Key,
                Params.VarSrctype);
        }

        // The client passes whether or not it registered a watch
        result.SetPass();
    }

    public override void Reset(Context context)
    {
        if (TryGetWatchedStore(context, out var store))
        {
            store.Unwatch(Key);
        }
    }

    private bool TryGetWatchedStore(Context context, out NamedVar store)
    {
        return context.TryGetNamedVarStore(Params.VarSrctype, allowGlobal: false, out store) && !store.IsReadOnly;
    }
}