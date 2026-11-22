using GameServer.StaticDB;
using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Logic;

public class CallCommand : Command, ICommand
{
    private CallCommandDef Params;

    public CallCommand(CallCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        if (context.AbilityId == Params.AbilityId)
        {
            Logger.Error("{Command} {CommandId}: ability {AbilityId} is calling itself, the call is cancelled", nameof(CallCommand), Params.Id, Params.AbilityId);
            result.SetFail(StatusCode.Error);
            return;
        }

        if (SDBInterface.GetAbilityData(Params.AbilityId) is not { } ability)
        {
            result.SetFail(StatusCode.Error);
            return;
        }

        if (ability.Chain == 0)
        {
            result.SetPass();
            return;
        }

        var chainResult = new CommandResult { Success = true };
        context.Abilities.Factory.LoadChain(ability.Chain).Execute(context, ref chainResult);
        result = chainResult;
    }

    public override void Reset(Context context)
    {
        if (context.AbilityId != Params.AbilityId && SDBInterface.GetAbilityData(Params.AbilityId) is { Chain: not 0 } ability)
        {
            ResetChain(context, ability.Chain);
        }
    }
}