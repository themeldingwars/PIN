using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using AeroMessages.GSS.V66.Character.Event;
using GameServer.Test;
using Google.Protobuf.Collections;
using GrpcGameServerAPIClient;

namespace GameServer.GRPC.EventHandlers;

public static class ArmyEventHandler
{
    private static readonly JsonSerializerOptions SerializerOptions
        = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    public static void HandleEvent(ArmyApplicationApproved e, IDictionary<uint, INetworkPlayer> clients)
    {
        SendMessageToCharacter(clients, e.CharacterGuid, "army_application_approve", e.InitiatorName);
    }

    public static void HandleEvent(ArmyApplicationReceived e, IDictionary<uint, INetworkPlayer> clients)
    {
        SendMessageToAuthorizedArmyMembers(clients, e.ArmyMemberGuids, "army_application", e.InitiatorName);
    }

    public static void HandleEvent(ArmyApplicationRejected e, IDictionary<uint, INetworkPlayer> clients)
    {
        SendMessageToCharacter(clients, e.CharacterGuid, "army_application_reject", e.InitiatorName);
    }

    public static void HandleEvent(ArmyApplicationsUpdated e, IDictionary<uint, INetworkPlayer> clients)
    {
        SendMessageToAuthorizedArmyMembers(clients, e.ArmyMemberGuids, "army_applications_change");
    }

    public static void HandleEvent(ArmyIdChanged e, IDictionary<uint, INetworkPlayer> clients)
    {
        var player = clients.Values.FirstOrDefault(p => p.CharacterId + 0xFE == e.CharacterGuid);

        if (player == null)
        {
            return;
        }

        var playerEntity = player.CharacterEntity;

        var staticInfo = playerEntity.StaticInfo;
        staticInfo.ArmyTag = DataUtils.FormatArmyTag(e.ArmyTag);

        playerEntity.SetStaticInfo(staticInfo);

        playerEntity.Character_BaseController.ArmyGUIDProp = e.ArmyGuid;
        playerEntity.Character_BaseController.ArmyIsOfficerProp = (sbyte)(e.IsOfficer ? 1 : 0);
    }

    public static void HandleEvent(ArmyInfoUpdated e, IDictionary<uint, INetworkPlayer> clients)
    {
        SendMessageToAllArmyMembers(clients, e.ArmyGuid, "army_info_change");
    }

    public static void HandleEvent(ArmyInviteApproved e, IDictionary<uint, INetworkPlayer> clients)
    {
        SendMessageToAllArmyMembers(clients, e.ArmyGuid, "army_invite_approve", e.InitiatorName);
    }

    public static void HandleEvent(ArmyInviteReceived e, IDictionary<uint, INetworkPlayer> clients)
    {
        var player = clients.Values.FirstOrDefault(p => p.CharacterId + 0xFE == e.CharacterGuid);

        if (player == null)
        {
            return;
        }

        var message = new ArmyMessage
                      {
                          message_type = "army_invite",
                          initiator = e.InitiatorName,
                          army = new Army { army_guid = e.ArmyGuid, name = e.ArmyName },
                          application = new Application { id = e.Id, message = e.Message }
                      };

        string json = JsonSerializer.Serialize(message, SerializerOptions);

        var msg = new ReceivedWebUIMessage() { Message = json };

        player.NetChannels[ChannelType.ReliableGss].SendMessage(msg, player.CharacterId);
    }

    public static void HandleEvent(ArmyInviteRejected e, IDictionary<uint, INetworkPlayer> clients)
    {
        SendMessageToAuthorizedArmyMembers(clients, e.ArmyMemberGuids, "army_invite_reject", e.InitiatorName);
    }

    public static void HandleEvent(ArmyMembersUpdated e, IDictionary<uint, INetworkPlayer> clients)
    {
        SendMessageToAllArmyMembers(clients, e.ArmyGuid, "army_characters_change");

        // Updating members could've changed officers displayed in army info
        SendMessageToAllArmyMembers(clients, e.ArmyGuid, "army_info_change");
    }

    public static void HandleEvent(ArmyRanksUpdated e, IDictionary<uint, INetworkPlayer> clients)
    {
        SendMessageToAllArmyMembers(clients, e.ArmyGuid, "army_rank_info_change");

        // // Updating ranks could've changed officers displayed in army info
        SendMessageToAllArmyMembers(clients, e.ArmyGuid, "army_info_change");
    }

    public static void HandleEvent(ArmyTagUpdated e, IDictionary<uint, INetworkPlayer> clients)
    {
        foreach (var armyMember in GetArmyMembers(clients, e.ArmyGuid))
        {
            var staticInfo = armyMember.CharacterEntity.StaticInfo;
            staticInfo.ArmyTag = DataUtils.FormatArmyTag(e.ArmyTag);

            armyMember.CharacterEntity.SetStaticInfo(staticInfo);
        }
    }

    private static IEnumerable<INetworkPlayer> GetArmyMembers(IDictionary<uint, INetworkPlayer> clients, ulong armyGuid)
    {
        return clients.Values.Where(p => p.CharacterEntity.Character_BaseController.ArmyGUIDProp == armyGuid);
    }

    private static void SendMessageToCharacter(
        IDictionary<uint, INetworkPlayer> clients, ulong characterGuid, string messageType, string initiatorName = null)
    {
        var player = clients.Values.FirstOrDefault(p => p.CharacterId + 0xFE == characterGuid);

        if (player == null)
        {
            return;
        }

        SendMessage(player, messageType, initiatorName);
    }

    private static void SendMessageToAllArmyMembers(
        IDictionary<uint, INetworkPlayer> clients,
        ulong armyGuid,
        string messageType,
        string initiatorName = null)
    {
        var armyMembers = GetArmyMembers(clients, armyGuid);

        foreach (var armyMember in armyMembers)
        {
            SendMessage(armyMember, messageType, initiatorName);
        }
    }

    private static void SendMessageToAuthorizedArmyMembers(
        IDictionary<uint, INetworkPlayer> clients,
        RepeatedField<ulong> armyMemberGuids,
        string messageType,
        string initiatorName = null)
    {
        var armyMembers = clients.Values.Where(p => armyMemberGuids.Contains(p.CharacterId + 0xFE));

        foreach (var armyMember in armyMembers)
        {
            SendMessage(armyMember, messageType, initiatorName);
        }
    }

    private static void SendMessage(INetworkPlayer player, string messageType, string initiatorName = null)
    {
        var armyMessage = new ArmyMessage { message_type = messageType, initiator = initiatorName };

        player.NetChannels[ChannelType.ReliableGss]
              .SendMessage(new ReceivedWebUIMessage() { Message = JsonSerializer.Serialize(armyMessage, SerializerOptions) },
                         player.CharacterId);
    }

#pragma warning disable SA1300 // Element should begin with upper-case letter
    private record ArmyMessage
    {
        public string      message_type { get; init; }
        public string      initiator    { get; init; }
        public Army        army         { get; init; }
        public Application application  { get; init; }
    }

    private record Army
    {
        public ulong  army_guid { get; init; }
        public string name      { get; init; }
    }

    private record Application
    {
        public ulong  id      { get; init; }
        public string message { get; init; }
    }
#pragma warning restore SA1300 // Element should begin with upper-case letter
}