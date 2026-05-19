using GameServer.StaticDB.Records.customdata;

namespace GameServer.Systems.Aptitude.Commands.Other;

public class RegisterHitTagTypeTriggerCommand : Command, ICommand
{
    private RegisterHitTagTypeTriggerCommandDef Params;

    public RegisterHitTagTypeTriggerCommand(RegisterHitTagTypeTriggerCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
    }
}