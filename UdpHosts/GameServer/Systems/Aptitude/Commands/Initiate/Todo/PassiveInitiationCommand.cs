using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Initiate;

public class PassiveInitiationCommand : Command, ICommand
{
    private PassiveInitiationCommandDef Params;

    public PassiveInitiationCommand(PassiveInitiationCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        // TODO: Handle Params.InitiationInterval
        context.InitTime ??= context.CurrentTime;
        result.SetPass();
    }
}