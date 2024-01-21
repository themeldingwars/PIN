using GameServer.Data.SDB.Records.apt;

namespace GameServer.Aptitude;

public class LogicAndChainCommand : ICommand
{
    private LogicAndChainCommandDef Params;

    public LogicAndChainCommand(LogicAndChainCommandDef par)
    {
        Params = par;
    }

    public bool Execute(Context context)
    {
        var chain = context.Abilities.Factory.LoadChain(Params.AndChain);

        bool result = chain.Execute(context, Chain.ExecutionMethod.AndChain);

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