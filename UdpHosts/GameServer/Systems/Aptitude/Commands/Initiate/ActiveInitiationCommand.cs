using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Initiate;

public class ActiveInitiationCommand : Command, ICommand
{
    private ActiveInitiationCommandDef Params;

    public ActiveInitiationCommand(ActiveInitiationCommandDef par)
    : base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        // The client waits for the activation input before it initiates, for us that input is the activation packet,
        // which already arrived when the chain runs. The AbilityActivated message is sent by the controller that received it.
        context.InitTime ??= context.CurrentTime;
        result.SetPass();
    }
}