using GameServer.StaticDB.Records;

namespace GameServer.Systems.Aptitude.Commands;

public abstract class BaseRegisterOpCommand : Command
{
    protected BaseRegisterOpCommand(ICommandDef def)
        : base(def)
    {
    }

    public override void Func1(Context context, ref CommandResult result)
    {
        Execute(context, ref result);
    }

    public override void Reset(Context context)
    {
        return;
    }
}