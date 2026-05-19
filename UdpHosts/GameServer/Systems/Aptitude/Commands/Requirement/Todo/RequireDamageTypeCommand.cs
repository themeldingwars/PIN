using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireDamageTypeCommand : Command, ICommand
{
    private RequireDamageTypeCommandDef Params;

    public RequireDamageTypeCommand(RequireDamageTypeCommandDef par)
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