using System;

namespace GameServer.Admin;

[ServerCommand("Print a list of all ServerCommands", "help", "help", "listcmd", "listcmds", "cmdlist", "cmds")]
public class ListCommandsServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        var admin = context.Shard.Admin;
        var message = admin.GetCommandList();
        if (context.SourcePlayer != null)
        {
            context.SourcePlayer.SendDebugLog(message);
            context.SourcePlayer.SendDebugChat("Command list printed to console");
        }
        else
        {
            Console.WriteLine(message);
        }
    }
}
