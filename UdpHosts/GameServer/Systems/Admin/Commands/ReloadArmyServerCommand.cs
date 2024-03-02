using AeroMessages.GSS.V66.Character.Event;

namespace GameServer.Admin;

[ServerCommand("Reload Army UI", "rarmy", "rarmy", "reloadarmy")]
public class ReloadArmyServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        string[] types =
        [
            "army_applications_change",
            "army_rank_info_change",
            "army_characters_change",
            "army_info_change"
        ];

        foreach (var type in types)
        {
            var msg = new ReceivedWebUIMessage() { Message = "{\"message_type\":\"" + type + "\"}" };

            context.SourcePlayer.NetChannels[ChannelType.ReliableGss]
                   .SendIAero(msg, context.SourcePlayer.CharacterEntity.EntityId);
        }
    }
}