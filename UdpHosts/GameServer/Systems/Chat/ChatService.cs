using System;
using System.Linq;
using AeroMessages.Common;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.Character.Command;
using AeroMessages.GSS.V66.Generic;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Enums;

namespace GameServer.Systems.Chat;

public class ChatService
{
    public static readonly ChatChannel[] PublicBroadcastChannels =
    {
        ChatChannel.Zone,
        ChatChannel.ZoneLang,
        ChatChannel.Say,
        ChatChannel.Yell,
    };

    private Shard Shard;
    private ChatCommandService CommandService;

    public ChatService(Shard shard)
    {
        Shard = shard;
        CommandService = new ChatCommandService(shard);
    }

    public void CharacterPerformTextChat(INetworkClient client, IEntity entity, PerformTextChat query)
    {
        if (query.Message.Length == 0 || query.AlternateData.AlternateType != ChatMessageAlternateData.ChatMessageAlternateType.NONE)
        {
            // TODO: AlternateType messages
            return;
        }
        
        ChatChannel queryChannel = (ChatChannel)query.Channel;

        var trimmed = query.Message.Trim();
        if (trimmed.StartsWith('\\'))
        {
            CommandService.ExecuteCommand(trimmed[1..], ((CharacterEntity)entity).Player);
            Console.WriteLine(query.Message);
        }
        else if (PublicBroadcastChannels.Contains(queryChannel))
        {
            SendToAll(query.Message, queryChannel, entity);
        }
        else if (queryChannel == ChatChannel.Admin)
        {
            Shard.Admin.ExecuteCommand(query.Message, ((CharacterEntity)entity).Player);
        }
        else
        {
            var player = ((CharacterEntity)entity).Player;
            player?.SendDebugChat("This channel is not available");
        }
    }

    public void SendToPlayer(string message, ChatChannel channel, INetworkClient player)
    {
        var response = PrepareSingleMessage(message, channel, null);
        player.NetChannels[ChannelType.UnreliableGss].SendMessage(response, Shard.InstanceId);
    }

    public void SendToAll(string message, ChatChannel channel, IEntity sender)
    {
        var response = PrepareSingleMessage(message, channel, sender);
        foreach (var client in Shard.Clients.Values)
        {
            if (client.Status.Equals(IPlayer.PlayerStatus.Playing))
            {
                client.NetChannels[ChannelType.UnreliableGss].SendMessage(response, Shard.InstanceId);
            }
        }
    }

    public string GetCommandList()
    {
        return CommandService.GetCommandList();
    }

    private ChatMessageList PrepareSingleMessage(string message, ChatChannel channel, IEntity sender)
    {
        var senderId = sender != null ? sender.AeroEntityId : new EntityId() { Backing = Shard.InstanceId };
        var senderName = sender != null ? ((CharacterEntity)sender).StaticInfo.DisplayName : "Server";
        byte chatIconFlags = (byte)(sender != null ? 0 : 1);

        var response = new ChatMessageList()
        {
            Messages =
            [
                new()
                {
                    SenderId = senderId,
                    SenderName = senderName,
                    Message = message,
                    Channel = (byte)channel,
                    ChatIconFlags = chatIconFlags,
                    AltData = new()
                    {
                        AlternateType = ChatMessageAlternateData.ChatMessageAlternateType.NONE,
                        HaveAltData = 0,
                    },
                    HaveAltEntity = 0,
                    HaveAltString = 0,
                },
            ],
        };
        return response;
    }
}