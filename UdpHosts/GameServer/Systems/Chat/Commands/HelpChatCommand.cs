using System;

namespace GameServer.Systems.Chat.Commands;

[ChatCommand("Print a list of all chat commands", "help", "help", "listcmd", "listcmds", "cmdlist", "cmds")]
public class HelpChatCommand : ChatCommand
{
    public override void Execute(string[] parameters, ChatCommandContext context)
    {
        var message = context.Shard.Chat.GetCommandList();
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
