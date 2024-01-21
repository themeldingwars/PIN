using System;
using System.Collections.Generic;

namespace GameServer.Aptitude;

public class Chain
{
    public uint Id = 0;
    public List<ICommand> Commands;

    public enum ExecutionMethod
    {
        AndChain,
        OrChain
    }

    public bool Execute(Context context, ExecutionMethod method = ExecutionMethod.AndChain) 
    {
        Console.WriteLine($"Executing Chain {Id}");
        if (method == ExecutionMethod.AndChain)
        {
            bool chainSuccess = true;
            foreach(var command in Commands)
            {
                Console.WriteLine($"Chain {Id} - Executing Command {command}");
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
                Console.WriteLine($"Chain {Id} - Executing Command {command}");
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
            Console.WriteLine($"- Command {command}");
        }
    }
}