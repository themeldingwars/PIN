using GameServer.StaticDB.Records.apt;

namespace GameServer.Systems.Aptitude.Commands.Logic;

public class ReturnCommand : Command, ICommand
{
    private ReturnCommandDef Params;

    public ReturnCommand(ReturnCommandDef par)
: base(par)
    {
        Params = par;
    }

    public override void Execute(Context context, ref CommandResult result)
    {
        result.Success = Params.ReturnSuccess != 0;
        result.Yield = Params.ReturnYield != 0;
        result.Halt = Params.ReturnHalt != 0;
        result.StatusCode = (StatusCode)Params.ReturnStatus;
    }
}