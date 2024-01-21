using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class LogicOrChainCommand : ICommand
{
    private LogicOrChainCommandDef Params;

    public LogicOrChainCommand(LogicOrChainCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var chain = context.Abilities.Factory.LoadChain(Params.OrChain);

        bool result = chain.Execute(context, Chain.ExecutionMethod.OrChain);

        if (Params.AlwaysSuccess == 1)
        {
            return true;
        }
        else
        {
            return result;
        }
    }
}