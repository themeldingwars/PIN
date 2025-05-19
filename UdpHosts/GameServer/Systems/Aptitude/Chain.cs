using System;
using System.Collections.Generic;

namespace GameServer.Aptitude;

public class Chain
{
    public uint Id = 0;
    public List<ICommand> Commands;

    public enum ExecutionMethod
    {
        /// <summary>
        /// Execution chain follows boolean AND logic. If a command returns false, early exit. Result of chain is only true if all commands succeeded.
        /// </summary>
        AndChain,

        /// <summary>
        /// Execution of chain follows boolean OR logic. If a command returns true, early exit. Result of chain is true if some command succeded.
        /// </summary>
        OrChain
    }

    public bool Execute(Context context, ExecutionMethod method = ExecutionMethod.AndChain)
    {
        bool debug = context.ExecutionHint is not(ExecutionHint.DurationEffect or ExecutionHint.UpdateEffect);

        if (debug)
        {
            context.Targets.TryPeek(out var res);
            Console.WriteLine($"Executing Chain {Id} ({context.ExecutionHint}), Self: {context.Self}, Initiator: {context.Initiator}, Target: {res?.ToString() ?? "none"}");
        }

        if (method == ExecutionMethod.AndChain)
        {
            bool chainSuccess = true;
            foreach(var command in Commands)
            {
                if (debug)
                {
                    var hasMoreInfo = command.ToString() != command.GetType().ToString();
                    Console.WriteLine($"Chain {Id} Command {command.Id} - Executing {(hasMoreInfo ? command : command.GetType().Name)}");
                }

                bool commandSuccess = command.Execute(context);
                if (!commandSuccess)
                {
                    chainSuccess = false;
                    break;
                }
            }

            return chainSuccess;
        }
        else if (method == ExecutionMethod.OrChain)
        {
            bool chainSuccess = false;
            foreach(var command in Commands)
            {
                if (debug)
                {
                    var hasMoreInfo = command.ToString() != command.GetType().ToString();
                    Console.WriteLine($"Chain {Id} Command {command.Id} - Executing {(hasMoreInfo ? command : command.GetType().Name)}");
                }

                bool commandSuccess = command.Execute(context);
                if (commandSuccess)
                {
                    chainSuccess = true;
                    break; // Note: Should further research to confirm if this is correct
                }
            }
    
            return chainSuccess;
        }
        
        return true;
    }

    public void DebugPrintCommands()
    {
        Console.WriteLine($"Chain {Id}");
        foreach(var command in Commands)
        {
            Console.WriteLine($"- Command {command.Id} {command}");
        }
    }
}
