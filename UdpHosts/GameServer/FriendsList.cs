using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AeroMessages.Common;
using AeroMessages.GSS;
using AeroMessages.GSS.Character.Event;
using AeroMessages.GSS.Generic;
using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.GRPC;
using GameServer.Protocol;
using GrpcGameServerAPIClient;
using Serilog;
using AltType = AeroMessages.GSS.ChatMessageAlternateData.ChatMessageAlternateType;
using UpdateFriendStatus = AeroMessages.GSS.Character.Command.UpdateFriendStatus;

namespace GameServer;

/// <summary>
///     What UpdateFriendStatus asks for, with the values RIN takes
/// </summary>
public enum FriendAction
{
    /// <summary>Sends a request, or accepts the friend's request</summary>
    Add = 1,

    /// <summary>Only replaces the note, the friendship stays as it is</summary>
    SetNote = 0,

    /// <summary>Ends a friendship, takes back a request or declines one</summary>
    Remove = -1,
}

/// <summary>
///     The friends list of a character. RIN keeps it, so every request is answered from there: a page is cut from the
///     whole list, and adding or removing a friend waits for RIN to say what changed.
///     A friendship starts as a request. The client sends UpdateFriendStatus with Add set both to ask and to accept,
///     and without it to remove a friend, take back a request and decline one, so RIN works out which it was from
///     what is on the lists. This game server tells its own player; RIN tells every game server what happened to the
///     other character, and the one hosting it tells that player. An offline player finds a request on its list at the
///     next login, and the client shows the invite for it then.
///     Whether a friend is online is presence the client keeps apart from the list: chat messages on the friends
///     channel from that friend add it (PlayerStateDetails) and drop it (PlayerDisconnected, PlayerTransferring).
///     The list UI takes a friend's status from there before the one on the list, and nothing else can change the
///     status of an entry the UI already shows, so this is also how an accepted request stops showing as pending.
///     Game servers report their players' presence to RIN, which passes it on to the friends wherever they are.
/// </summary>
public static class FriendsList
{
    // The client's friends_list.per_page defaults to 50, it asks for the next page after a full one
    private const int PageSize = 50;

    // The reasons RIN answers UpdateCharacterFriend with
    private const string ReasonRequested = "REQUESTED";
    private const string ReasonAccepted = "ACCEPTED";
    private const string ReasonUnchanged = "UNCHANGED";
    private const string ReasonRemoved = "REMOVED";
    private const string ReasonCancelled = "CANCELLED";
    private const string ReasonDeclined = "DECLINED";
    private const string ReasonNotFound = "NOT_FOUND";
    private const string ReasonSelf = "SELF";

    // What a FriendNotification from RIN tells its targets
    private const uint NotificationRequest = 1;
    private const uint NotificationDeclined = 2;
    private const uint NotificationRemoved = 3;
    private const uint NotificationAccepted = 4;
    private const uint NotificationOnline = 5;
    private const uint NotificationOffline = 6;
    private const uint NotificationFriendLogin = 7;
    private const uint NotificationFriendLogout = 8;
    private const uint NotificationArmyLogin = 9;
    private const uint NotificationArmyLogout = 10;

    // HTTP style codes in FriendsListChanged.StatusCode. The client only tells 404 apart, as "Not Found".
    private const uint StatusOk = 200;
    private const uint StatusBadRequest = 400;
    private const uint StatusNotFound = 404;
    private const uint StatusError = 500;

    // PlayerStateDetails without its DataLength and ServerHost: the bytes of every fixed size field, and the 0 that ends
    // the string. DataLength counts the bytes after it.
    private const int StateDetailsFixedLength = 37;

    // Starts and ends a chat link (ChatLib's c_Endcap): "~" and the control characters 0x02 0x1E. A plain "~" shows as text.
    private const string LinkEndcap = "~\u0002\u001e";

    // The UI lets a note be 20 characters long, this only keeps a modified client from storing more
    private const int MaxNoteLength = 64;

    private static readonly ILogger _logger = Log.ForContext(typeof(FriendsList));

    // ChatLib.StripArmyTag
    private static readonly Regex ArmyTag = new(@"\s*\[.+\]\s*", RegexOptions.Compiled);

    /// <summary>
    ///     What FriendsListChanged tells the client. Added raises ON_FRIEND_ADDED and shows the friend as pending,
    ///     Removed raises ON_FRIEND_REMOVED, Updated raises ON_FRIEND_STATUS_CHANGED.
    /// </summary>
    private enum Change
    {
        Added,
        Removed,
        Updated,
    }

    /// <summary>
    ///     Answers FriendsListRequest. The page past the last friend comes back empty, which ends the list.
    ///     The client asks for the first page when its UI is ready, which is when the player gets the presence of its
    ///     friends: the UI keeps an entry it made from presence over the one in the list, without the note, so
    ///     presence must not arrive before the list.
    /// </summary>
    public static void SendPage(INetworkPlayer player, uint page)
    {
        _ = SendPageAsync(player, page);
    }

    /// <summary>
    ///     Answers UpdateFriendStatus with FriendsListChanged. RIN tells the other player.
    /// </summary>
    public static void Update(INetworkPlayer player, UpdateFriendStatus request)
    {
        // Names the UI took from chat or a player menu can carry the army tag, which the client strips the same way
        var name = ArmyTag.Replace(request.TargetName ?? string.Empty, string.Empty).Trim();
        var note = request.HaveNote == 1 ? request.Note ?? string.Empty : null;
        if (note?.Length > MaxNoteLength)
        {
            note = note[..MaxNoteLength];
        }

        // Friends.Add and Friends.Accept send Add 1, Friends.Remove and Friends.Decline Add 0, and Friends.SetNote
        // Add 0 with a note
        var action = request.Add > 0 ? FriendAction.Add : note != null ? FriendAction.SetNote : FriendAction.Remove;

        _logger.Information("UpdateFriendStatus from {CharacterId:X}: {Name}, Add {Add}, HaveNote {HaveNote}: {Action}", player.CharacterId, name, request.Add, request.HaveNote, action);

        if (player.CharacterEntity == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(name))
        {
            SendFailed(player, request.TargetName ?? string.Empty, StatusNotFound);
            return;
        }

        _ = UpdateAsync(player, name, action, note);
    }

    /// <summary>
    ///     Tells RIN, and through it the player's friends wherever they are, that the player is online and where.
    ///     Called on the player's shard thread whenever it entered a zone, so a move between shards shows as well.
    /// </summary>
    /// <param name="player">The player that is online</param>
    /// <param name="sendFriends">Also send the player the presence of its online friends, only once it has its list</param>
    /// <param name="resync">Reported again for a new stream, the player didn't just log in</param>
    public static void ReportOnline(INetworkPlayer player, bool sendFriends = false, bool resync = false)
    {
        var character = player.CharacterEntity;
        if (character == null)
        {
            return;
        }

        _ = GRPCService.SendCommandAsync(new Command
        {
            SetFriendPresence = new SetFriendPresence
            {
                CharacterGuid = CharacterGuid(player),
                Name = character.StaticInfo.DisplayName,
                Presence = PresenceOf(player, character),
                SendFriends = sendFriends,
                Resync = resync,
            },
        });
    }

    /// <summary>
    ///     Tells RIN the player is leaving this game server. Called before the character leaves its shard. RIN ignores
    ///     it when the character has already come online elsewhere. Friends and army are told about the logout at
    ///     once when the player logged out, and only if they stay away for a while after a zone transfer, whose relog
    ///     follows shortly, or a timeout or crash, which the player may come straight back from.
    /// </summary>
    public static void ReportOffline(INetworkPlayer player)
    {
        var character = player.CharacterEntity;
        if (character == null)
        {
            return;
        }

        _ = GRPCService.SendCommandAsync(new Command
        {
            SetFriendPresence = new SetFriendPresence
            {
                CharacterGuid = CharacterGuid(player),
                Name = character.StaticInfo.DisplayName,
                MayReturn = player.TransferringZone || !player.LoggedOut,
            },
        });
    }

    /// <summary>
    ///     Reports every player again, for a new stream to RIN: presence lives as long as the stream that reported it.
    ///     Players on a game server have their lists, so they also get their friends that came online in between.
    /// </summary>
    public static void ReportAllOnline(IShardManager shards)
    {
        foreach (var shard in shards.Shards)
        {
            shard.Post(() =>
            {
                foreach (var player in shard.Clients.Values)
                {
                    ReportOnline(player, true, true);
                }
            });
        }
    }

    /// <summary>
    ///     Tells the targets of a notification from RIN that are on this shard. Called on the shard's thread.
    /// </summary>
    public static void HandleNotification(FriendNotification notification, IDictionary<uint, INetworkPlayer> clients)
    {
        var from = Sender.Of(notification.FromGuid, notification.FromName);
        var targets = notification.TargetGuids.Select(guid => guid & 0xffffffffffffff00).ToHashSet();

        foreach (var player in clients.Values.Where(p => p.CharacterEntity != null && targets.Contains(p.CharacterId)))
        {
            switch (notification.Kind)
            {
                case NotificationRequest:
                    // Presence outlives a friendship on the client and would show over the new request
                    SendFriendChat(player, from, TransferringAltData(), RequestAltData(true));
                    break;
                case NotificationDeclined:
                    SendFriendChat(player, from, TransferringAltData(), RequestAltData(false));
                    break;
                case NotificationRemoved:
                    SendFriendChat(player, from, TransferringAltData());
                    SendChanged(player, from.Name, Change.Removed);
                    _ = RefreshFriendCountAsync(player);
                    break;
                case NotificationAccepted:
                    SendChanged(player, from.Name, Change.Updated);
                    if (notification.FromPresence != null)
                    {
                        SendFriendChat(player, from, TransferringAltData(), StateDetailsAltData(notification.FromPresence));
                    }

                    _ = RefreshFriendCountAsync(player);
                    break;
                case NotificationOnline when notification.FromPresence != null:
                    // The client keeps the first details it gets for a friend, dropping the old ones first makes these count
                    SendFriendChat(player, from, TransferringAltData(), StateDetailsAltData(notification.FromPresence));
                    break;
                case NotificationOffline:
                    SendFriendChat(player, from, DisconnectedAltData());
                    break;
                case NotificationFriendLogin:
                    SendNotice(player, ChatChannel.Friends, $"{PlayerLink(from.Name)} is now online.");
                    break;
                case NotificationFriendLogout:
                    SendNotice(player, ChatChannel.Friends, $"{PlayerLink(from.Name)} is now offline.");
                    break;
                case NotificationArmyLogin:
                    SendNotice(player, ChatChannel.Army, $"{PlayerLink(from.Name)} is now online.");
                    break;
                case NotificationArmyLogout:
                    SendNotice(player, ChatChannel.Army, $"{PlayerLink(from.Name)} is now offline.");
                    break;
            }
        }
    }

    private static async Task SendPageAsync(INetworkPlayer player, uint page)
    {
        var friends = await FetchAsync(CharacterGuid(player));

        Post(player, () => SendPageNow(player, page, friends));
    }

    private static async Task UpdateAsync(INetworkPlayer player, string name, FriendAction action, string note)
    {
        UpdateCharacterFriendResp resp;
        try
        {
            resp = await GRPCService.UpdateCharacterFriendAsync(CharacterGuid(player), name, action, note);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Couldn't {Action} friend {Name} for {CharacterId:X}", action, name, player.CharacterId);
            Post(player, () => SendFailed(player, name, StatusError));
            return;
        }

        _logger.Information("{Action} friend {Name} for {CharacterId:X}: {Reason}", action, name, player.CharacterId, resp.Reason);

        // Answer with the name as it is spelled on the character, not as it was typed
        var friendName = resp.Friend?.Name ?? name;
        var friend = resp.Friend != null ? Sender.Of(resp.Friend.CharacterGuid, friendName) : default;

        // Presence outlives a friendship on the client and would show over a new request, so whenever an entry goes or
        // a request goes out, the other character is dropped from presence first
        switch (resp.Reason)
        {
            case ReasonRequested:
                Post(player, () =>
                {
                    SendFriendChat(player, friend, TransferringAltData());
                    SendChanged(player, friendName, Change.Added);
                });
                break;
            case ReasonAccepted:
                // The entry still shows the request until presence replaces it
                Post(player, () =>
                {
                    SendChanged(player, friendName, Change.Added);
                    if (resp.FriendPresence != null)
                    {
                        SendFriendChat(player, friend, TransferringAltData(), StateDetailsAltData(resp.FriendPresence));
                    }
                    else
                    {
                        SendOfflineStatus(player, friend);
                    }
                });
                break;
            case ReasonUnchanged:
                Post(player, () => SendChanged(player, friendName, Change.Updated));
                return;
            case ReasonRemoved:
            case ReasonCancelled:
            case ReasonDeclined:
                Post(player, () =>
                {
                    SendFriendChat(player, friend, TransferringAltData());
                    SendChanged(player, friendName, Change.Removed);
                });
                break;
            case ReasonNotFound:
                Post(player, () => SendFailed(player, name, StatusNotFound));
                return;
            case ReasonSelf:
                Post(player, () => SendFailed(player, name, StatusBadRequest));
                return;
            default:
                Post(player, () => SendFailed(player, name, StatusError));
                return;
        }

        // Only friends count, so the count changes when a friendship starts or ends
        await RefreshFriendCountAsync(player);
    }

    /// <summary>
    ///     The whole list from RIN, or null when it couldn't be fetched
    /// </summary>
    private static async Task<CharacterFriendsResponse> FetchAsync(ulong characterGuid)
    {
        try
        {
            return await GRPCService.GetCharacterFriendsAsync((long)characterGuid);
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Could not fetch the friends list of {CharacterGuid:X} over GRPC", characterGuid);
            return null;
        }
    }

    private static async Task RefreshFriendCountAsync(INetworkPlayer player)
    {
        var friends = await FetchAsync(CharacterGuid(player));
        if (friends != null)
        {
            Post(player, () => player.CharacterEntity?.SetFriendCount(CountFriends(friends)));
        }
    }

    private static void SendPageNow(INetworkPlayer player, uint page, CharacterFriendsResponse friends)
    {
        var character = player.CharacterEntity;
        if (character == null)
        {
            return;
        }

        // A list that couldn't be fetched goes out empty, so the client isn't left waiting for it
        var all = friends?.Friends.ToList() ?? [];
        if (friends != null)
        {
            character.SetFriendCount(CountFriends(friends));
        }

        var response = new FriendsListResponse
        {
            Friends = [.. all
                .Skip((int)Math.Min((long)page * PageSize, int.MaxValue))
                .Take(PageSize)
                .Select(f => new FriendsListData
                {
                    CharacterGuid = f.CharacterGuid,
                    Name = f.Name,
                    Note = f.Note,
                    StatusType = (FriendStatusType)f.Status,
                    LastSeenAt = (uint)Math.Clamp(f.LastSeenAt, 0, uint.MaxValue),
                    IsOnline = (byte)(f.IsOnline ? 1 : 0),
                })],
            Page = page,
        };

        player.NetChannels[ChannelType.ReliableGss].SendMessage(response, character.EntityId);

        // Presence comes back through RIN after the list, which went out first on the same channel
        if (page == 0)
        {
            ReportOnline(player, true);
        }
    }

    private static void SendChanged(INetworkPlayer player, string name, Change change)
    {
        var character = player.CharacterEntity;
        if (character == null)
        {
            return;
        }

        var changed = new FriendsListChanged
        {
            Name = name,
            StatusCode = StatusOk,
            Added = (byte)(change == Change.Added ? 1 : 0),
            Removed = (byte)(change == Change.Removed ? 1 : 0),
            Success = 1,
        };

        player.NetChannels[ChannelType.ReliableGss].SendMessage(changed, character.EntityId);
    }

    /// <summary>
    ///     Raises ON_FRIEND_REQUEST_ERROR on the client
    /// </summary>
    private static void SendFailed(INetworkPlayer player, string name, uint status)
    {
        var character = player.CharacterEntity;
        if (character == null)
        {
            return;
        }

        var changed = new FriendsListChanged { Name = name, StatusCode = status, Added = 0, Removed = 0, Success = 0 };
        player.NetChannels[ChannelType.ReliableGss].SendMessage(changed, character.EntityId);
    }

    /// <summary>
    ///     Makes the entry of an offline friend show as offline. The client only replaces the status of an entry it
    ///     already shows through presence, and it shows a friend that just went offline as offline, so the friend
    ///     comes online and leaves again within one message.
    /// </summary>
    private static void SendOfflineStatus(INetworkPlayer player, Sender friend)
    {
        SendFriendChat(player, friend, TransferringAltData(), StateDetailsAltData(new FriendPresence()), DisconnectedAltData());
    }

    /// <summary>
    ///     Sends chat messages on the friends channel from another character. Requests, their answers and presence
    ///     all travel this way; the client raises friend events for them instead of showing text. All of it goes on
    ///     the same channel as FriendsListChanged, so it arrives in the order it was sent.
    /// </summary>
    private static void SendFriendChat(INetworkPlayer player, Sender from, params ChatMessageAlternateData[] altData)
    {
        var shard = player.AssignedShard;
        if (player.CharacterEntity == null || shard == null)
        {
            return;
        }

        var message = new ChatMessageList
        {
            Messages = [.. altData.Select(data => new ChatMessageData
            {
                SenderId = from.EntityId,
                SenderName = from.Name,
                Message = string.Empty,

                // The client only takes presence from the friends channel
                Channel = (byte)ChatChannel.Friends,
                ChatIconFlags = 0,
                AltData = data,
                HaveAltEntity = 0,
                HaveAltString = 0,
            })],
        };

        player.NetChannels[ChannelType.ReliableGss].SendMessage(message, shard.InstanceId);
    }

    /// <summary>
    ///     A line of text on a chat channel. The client has no message of its own for friends and army mates logging
    ///     in or out: its "joined"/"left" lines (ON_PLAYER_JOINED_CHAT_CHANNEL) are registered but nothing in the
    ///     1962 client raises them any more. Without an author the line shows without the "name:" in front.
    /// </summary>
    private static void SendNotice(INetworkPlayer player, ChatChannel channel, string text)
    {
        var shard = player.AssignedShard;
        if (player.CharacterEntity == null || shard == null)
        {
            return;
        }

        var message = new ChatMessageList
        {
            Messages =
            [
                new ChatMessageData
                {
                    SenderId = new EntityId { Backing = shard.InstanceId },
                    SenderName = string.Empty,
                    Message = text,
                    Channel = (byte)channel,
                    ChatIconFlags = 0,
                    AltData = new ChatMessageAlternateData { AlternateType = AltType.NONE, HaveAltData = 0 },
                    HaveAltEntity = 0,
                    HaveAltString = 0,
                },
            ],
        };

        player.NetChannels[ChannelType.ReliableGss].SendMessage(message, shard.InstanceId);
    }

    // ChatLib.EncodePlayerLink: the chat turns it into the clickable name that opens the player menu
    private static string PlayerLink(string name) => $"{LinkEndcap}p|{name}{LinkEndcap}";

    /// <summary>
    ///     A request raises ON_FRIEND_REQUEST_RECEIVED, which shows the invite; the answer that declines one raises
    ///     ON_FRIEND_REQUEST_DECLINED.
    /// </summary>
    private static ChatMessageAlternateData RequestAltData(bool isRequest) => new()
    {
        AlternateType = AltType.FriendRequestOrRemove,
        HaveAltData = 1,
        AltData_FriendRequestOrRemove = new ChatAltData_FriendRequestOrRemove { DataLength = 1, IsRequest = (sbyte)(isRequest ? 1 : 0) },
    };

    // Drops the friend from the client's presence and shows it offline
    private static ChatMessageAlternateData DisconnectedAltData() => new()
    {
        AlternateType = AltType.PlayerDisconnected,
        HaveAltData = 1,
        AltData_PlayerDisconnected = new ChatAltData_PlayerDisconnected { DataLength = 0, Data = [] },
    };

    // Drops the friend from the client's presence without an event
    private static ChatMessageAlternateData TransferringAltData() => new()
    {
        AlternateType = AltType.PlayerTransferring,
        HaveAltData = 1,
        AltData_PlayerTransferring = new ChatAltData_PlayerTransferring { DataLength = 0 },
    };

    /// <summary>
    ///     Adds the friend to the client's presence: Friends.GetOnline in the UI shows the zone, the battleframe and the
    ///     instance.
    /// </summary>
    private static ChatMessageAlternateData StateDetailsAltData(FriendPresence presence)
    {
        var serverHost = string.Empty;

        return new ChatMessageAlternateData
        {
            AlternateType = AltType.PlayerStateDetails,
            HaveAltData = 1,
            AltData_PlayerStateDetails = new ChatAltData_PlayerStateDetails
            {
                DataLength = (ushort)(StateDetailsFixedLength + Encoding.UTF8.GetByteCount(serverHost)),
                Gender = (byte)presence.Gender,
                ChassisId = presence.ChassisId,
                Level = (byte)presence.Level,
                EffectiveLevel = (byte)presence.EffectiveLevel,
                EliteRank = presence.EliteRank,
                InstanceId = presence.InstanceId,
                ZoneId = presence.ZoneId,
                ServerHost = serverHost,
                PvPRank = 0,
                HaveUnk10 = 0,
                ChatIconFlags = 0,
                CharacterState = 0,
                MaxHealth = presence.MaxHealth,
                NameCardId = 0,
            },
        };
    }

    // What the friends of a player see of it
    private static FriendPresence PresenceOf(INetworkPlayer player, CharacterEntity character) => new()
    {
        Gender = character.StaticInfo.Gender,
        ChassisId = character.CurrentLoadout?.ChassisID ?? 0,
        Level = character.Level,
        EffectiveLevel = character.EffectiveLevel,
        EliteRank = character.EliteLevel,
        InstanceId = player.AssignedShard?.InstanceId ?? 0,
        ZoneId = player.CurrentZone?.ID ?? 0,
        MaxHealth = (uint)Math.Max(0, character.MaxHealth.Value),
    };

    private static ushort CountFriends(CharacterFriendsResponse friends)
    {
        var count = friends.Friends.Count(f => f.Status == (uint)FriendStatusType.Friend);
        return (ushort)Math.Min(count, ushort.MaxValue);
    }

    // Sending to a player and changing its character is only done on the thread of the shard it is on
    private static void Post(INetworkPlayer player, Action action)
    {
        player?.AssignedShard?.Post(action);
    }

    // RIN identifies characters by the full guid, which has the character type in its last byte
    private static ulong CharacterGuid(INetworkPlayer player) => player.CharacterId + (byte)GuidService.AdditionalTypes.Character;

    /// <summary>
    ///     A character that friend messages come from, it doesn't have to be on this game server. The client keys a
    ///     friend's presence by this name.
    /// </summary>
    private readonly record struct Sender(EntityId EntityId, string Name)
    {
        public static Sender Of(ulong characterGuid, string name) =>
            new(new EntityId { Backing = characterGuid & 0xffffffffffffff00, ControllerId = Controller.Generic }, name);
    }
}
