using System;
using System.Linq;
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

        Console.WriteLine($"LogicAndChainCommand Pre Target {context.Targets.FirstOrDefault()}");

        var prevExecutionHint = context.ExecutionHint;
        context.ExecutionHint = ExecutionHint.Logic;
        bool result = chain.Execute(context, Chain.ExecutionMethod.AndChain);
        context.ExecutionHint = prevExecutionHint;

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