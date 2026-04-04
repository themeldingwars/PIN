using System.Collections.Generic;
using Serilog;

namespace GameServer.Aptitude;

public class Chain
{
    private static readonly ILogger _logger = Log.ForContext<Chain>();

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
        using var logContext = Serilog.Context.LogContext.PushProperty("ExecutionId", context.ExecutionId);
        bool debug = context.ExecutionHint is not (ExecutionHint.DurationEffect or ExecutionHint.UpdateEffect);

        if (debug)
        {
            context.Targets.TryPeek(out var res);
            _logger.Debug("Executing Chain {ChainId} ({ExecutionHint}), Self: {Self}, Initiator: {Initiator}, Target: {Target}", Id, context.ExecutionHint, context.Self, context.Initiator, res?.ToString() ?? "none");
        }

        if (method == ExecutionMethod.AndChain)
        {
            bool chainSuccess = true;
            foreach(var command in Commands)
            {
                if (debug)
                {
                    var hasMoreInfo = command.ToString() != command.GetType().ToString();
                    _logger.Debug("Chain {ChainId} Command {CommandId} - Executing {CommandName}", Id, command.Id, hasMoreInfo ? command : command.GetType().Name);
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
                    _logger.Debug("Chain {ChainId} Command {CommandId} - Executing {CommandName}", Id, command.Id, hasMoreInfo ? command : command.GetType().Name);
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
        Log.Information("Chain {ChainId}", Id);
        if (Commands.Count == 0)
        {
            Log.Warning("Loaded empty chain {ChainId}", Id);
        }
        foreach(var command in Commands)
        {
            Log.Information("- Command {CommandId} {Command}", command.Id, command);
        }
    }
}