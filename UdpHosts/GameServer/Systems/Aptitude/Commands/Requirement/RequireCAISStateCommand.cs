using AeroMessages.GSS.Character.Controller;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.aptfs;

namespace GameServer.Systems.Aptitude.Commands.Requirement;

public class RequireCAISStateCommand : Command, ICommand
{
    private RequireCAISStateCommandDef Params;

    public RequireCAISStateCommand(RequireCAISStateCommandDef par)
        : base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        bool cmdResult = false;

        var target = context.Self;

        if (target is not CharacterEntity character)
        {
            Logger.Warning("{Command} {CommandId} fails because target is not a Character. If this is happening, we should investigate why.", nameof(RequireCAISStateCommand), Params.Id);
            result.SetFail();
            return;
        }

        var state = character.Character_BaseController.CAISStatusProp.State;

        if (Params.None == 1)
        {
            cmdResult = state == CAISStatusData.CAISState.None;
        }

        if (Params.Fatigued == 1)
        {
            cmdResult = cmdResult || state == CAISStatusData.CAISState.Fatigued;
        }

        if (Params.Unhealthy == 1)
        {
            cmdResult = cmdResult || state == CAISStatusData.CAISState.Unhealthy;
        }

        if (Params.Healthy == 1)
        {
            cmdResult = cmdResult || state == CAISStatusData.CAISState.Healthy;
        }

        if (cmdResult)
        {
            result.SetPass();
        }
        else
        {
            result.SetFail();
        }

        return;
    }

    public override void Reset(Context context)
    {
        return;
    }
}