using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class LogicOrCommand : ICommand
{
    private LogicOrCommandDef Params;

    public LogicOrCommand(LogicOrCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var chainA = context.Abilities.Factory.LoadChain(Params.AChain);
        var chainB = context.Abilities.Factory.LoadChain(Params.BChain);

        bool resultA = chainA.Execute(context);
        bool resultB = chainB.Execute(context);

        bool result = resultA || resultB;

        return result;
    }
}