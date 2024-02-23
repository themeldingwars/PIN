using System;
using System.Linq;
using System.Threading;
using AeroMessages.Common;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.Character;
using AeroMessages.GSS.V66.Character.Command;
using AeroMessages.GSS.V66.Character.Event;
using AeroMessages.GSS.V66.Generic;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Enums;

namespace GameServer;

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

    public ChatService(Shard shard)
    {
        Shard = shard;
    }

    public void CharacterPerformTextChat(INetworkClient client, IEntity entity, PerformTextChat query)
    {
        if (query.Message.Length == 0 || query.AlternateData.AlternateType != ChatMessageAlternateData.ChatMessageAlternateType.NONE)
        {
            // TODO: AlternateType messages
            return;
        }
        
        ChatChannel queryChannel = (ChatChannel)query.Channel;

        if (PublicBroadcastChannels.Contains(queryChannel))
        {
            SendToAll(query.Message, queryChannel, entity);
        }
        else if (queryChannel == ChatChannel.Admin)
        {
            // TODO: ParseCommand
        }
        else
        {
            var player = ((CharacterEntity)entity).Player;
            if (player != null)
            {
                SendToPlayer("This channel is not available", ChatChannel.Debug, player);
            }
        }
    }

    public void SendToPlayer(string message, ChatChannel channel, INetworkPlayer player)
    {
        var response = PrepareSingleMessage(message, channel, null);
        player.NetChannels[ChannelType.UnreliableGss].SendIAero(response, Shard.InstanceId);
    }

    public void SendToAll(string message, ChatChannel channel, IEntity sender)
    {
        var response = PrepareSingleMessage(message, channel, sender);
        foreach (var client in Shard.Clients.Values)
        {
            if (client.Status.Equals(IPlayer.PlayerStatus.Playing))
            {
                client.NetChannels[ChannelType.UnreliableGss].SendIAero(response, Shard.InstanceId);
            }
        }
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