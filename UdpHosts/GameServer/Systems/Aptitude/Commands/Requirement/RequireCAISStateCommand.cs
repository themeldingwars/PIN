using System;
using AeroMessages.GSS.V66.Character.Controller;
using GameServer.Data.SDB.Records.aptfs;
using GameServer.Entities.Character;

namespace GameServer.Aptitude;

public class RequireCAISStateCommand : Command, ICommand
{
    private RequireCAISStateCommandDef Params;

    public RequireCAISStateCommand(RequireCAISStateCommandDef par)
        : base(par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        bool result = false;

        var target = context.Self;

        if (target is not CharacterEntity character)
        {
            Console.WriteLine("RequireCAISStateCommand fails because target is not a Character. If this is happening, we should investigate why.");
            return false;
        }

        var state = character.Character_BaseController.CAISStatusProp.State;

        if (Params.None == 1)
        {
            result = state == CAISStatusData.CAISState.None;
        }

        if (Params.Fatigued == 1)
        {
            result = result || state == CAISStatusData.CAISState.Fatigued;
        }

        if (Params.Unhealthy == 1)
        {
            result = result || state == CAISStatusData.CAISState.Unhealthy;
        }

        if (Params.Healthy == 1)
        {
            result = result || state == CAISStatusData.CAISState.Healthy;
        }

        return result;
    }
}