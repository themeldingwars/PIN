using System;
using System.Collections.Generic;
using System.Numerics;
using AeroMessages.Common;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.Character;
using AeroMessages.GSS.V66.Character.Command;
using AeroMessages.GSS.V66.Character.Controller;
using AeroMessages.GSS.V66.Character.Event;
using AeroMessages.GSS.V66.Character.View;
using AeroMessages.GSS.V66.Vehicle;
using GameServer.Entities.Character;
using GameServer.Enums.GSS.Character;
using GameServer.Extensions;
using GameServer.Packets;
using Serilog;
using VController = AeroMessages.GSS.V66.Vehicle.Controller;

namespace GameServer.Controllers.Character;

[ControllerID(Enums.GSS.Controllers.Character_BaseController)]
public class BaseController : Base
{
    private ILogger _logger;

    public override void Init(INetworkClient client, IPlayer player, IShard shard, ILogger logger)
    {
        _logger = logger;
    }

    [MessageID((byte)Commands.FetchQueueInfo)]
    public void FetchQueueInfo(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var fetchQueueInfoResponse = new FetchQueueInfoResponse
        {
            Succes = 0,
            Queues = new FetchQueueData[]
            {
                new FetchQueueData
                {
                    QueueId = 2121,
                    Qualifies = 1,
                    ChallengeEnabled = 0,
                    Gametype = "campaign",
                    DisplayKeyName = "MATCH_MAP_PROVING_GROUND",
                    DisplayKeyDesc = "MATCH_MAP_PROVING_GROUND_DESC",
                    ZoneId = 1155,
                    MissionId = 497,
                    Certs = new QueueCertsData[]
                    {
                        new QueueCertsData
                        {
                            CertId = 3589,
                            Passed = 1
                        }
                    },
                    Difficulties = new QueueDifficultiesData[]
                    {
                        new QueueDifficultiesData
                        {
                            DifficultyId = 6922,
                            UiString = "INSTANCE_DIFFICULTY_NORMAL",
                            MinLevel = 20,
                            DisplayLevel = 20,
                            MaxSuggestedLevel = 40,
                            DifficultyKey = "NORMAL_MODE",
                            PlayerCount1 = 5,
                            PlayerCount2 = 5,
                            PlayerCount3 = 5,
                            MinPlayers = 1,
                            MaxPlayers = 5
                        },
                        new QueueDifficultiesData
                        {
                            DifficultyId = 7022,
                            UiString = "INSTANCE_DIFFICULTY_CHALLENGE",
                            MinLevel = 20,
                            DisplayLevel = 20,
                            MaxSuggestedLevel = 40,
                            DifficultyKey = "CHALLENGE_MODE",
                            PlayerCount1 = 5,
                            PlayerCount2 = 5,
                            PlayerCount3 = 5,
                            MinPlayers = 1,
                            MaxPlayers = 5
                        },
                        new QueueDifficultiesData
                        {
                            DifficultyId = 9122,
                            UiString = "INSTANCE_DIFFICULTY_HARD",
                            MinLevel = 45,
                            DisplayLevel = 45,
                            MaxSuggestedLevel = 45,
                            DifficultyKey = "HARD_MODE",
                            PlayerCount1 = 5,
                            PlayerCount2 = 5,
                            PlayerCount3 = 5,
                            MinPlayers = 1,
                            MaxPlayers = 5
                        }
                    },
                    RewardsWinnerItems = new QueueRewardsItemData[] { },
                    RewardsWinnerLoots = new QueueRewardsLootData[]
                    {
                        new QueueRewardsLootData { LootTableId = 10133, DifficultyKey = "CHALLENGE_MODE" },
                        new QueueRewardsLootData { LootTableId = 10134, DifficultyKey = "HARD_MODE" },
                        new QueueRewardsLootData { LootTableId = 10132, DifficultyKey = "NORMAL_MODE" }
                    },
                    RewardsLooserItems = new QueueRewardsItemData[] { },
                    RewardsLooserLoots = new QueueRewardsLootData[] { }
                }
            }
        };
    }

    [MessageID((byte)Commands.PlayerReady)]
    public void PlayerReady(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        player.Ready();
    }

    [MessageID((byte)Commands.MovementInput)]
    public void MovementInput(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // ToDo: This currently only handles PosRotState inputs, logic needs to be added for the other MovementDataTypes
        if (packet.BytesRemaining < 64)
        {
            return;
        }

        var movementInput = packet.Unpack<AeroMessages.GSS.V66.Character.Command.MovementInput>();

        if (!player.CharacterEntity.Alive)
        {
            return; // can't move if you're dead (or at least shouldn't o.o")
        }

        client.AssignedShard.Movement.CharacterMovementInput(client, player.CharacterEntity, movementInput);
    }

    [MessageID((byte)Commands.SetMovementSimulation)]
    public void SetMovementSimulation(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        // Dear client, thanks for informing us about how often you will update us.
        // We will continue to rely on your support, whilst doing nothing ourselves.
        // Best regards, TMW
        // var setMovementSimulation = packet.Unpack<SetMovementSimulation>();
        // LogMissingImplementation<BaseController>(nameof(SetMovementSimulation), entityId, packet, _logger);
    }

    [MessageID((byte)Commands.BagInventorySettings)]
    public void BagInventorySettings(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var bagInventoryUpdate = new BagInventoryUpdate
        {
            Data = "{\"version\":2,\"bag_types\":[{\"definitions\":[{\"name\":\"\",\"length\":25,\"accept_types\":[]},{\"name\":\"\",\"length\":25,\"accept_types\":[]},{\"name\":\"\",\"length\":25,\"accept_types\":[]},{\"name\":\"\",\"length\":25,\"accept_types\":[]},{\"name\":\"\",\"length\":25,\"accept_types\":[]},{\"name\":\"\",\"length\":25,\"accept_types\":[]},{\"name\":\"\",\"length\":25,\"accept_types\":[]},{\"name\":\"\",\"length\":25,\"accept_types\":[]},{\"name\":\"\",\"length\":25,\"accept_types\":[]}],\"slots\":[{\"item_guid\":744967187901210112,\"item_sdb_id\":130370,\"quantity\":1},{\"item_guid\":1536551767489147648,\"item_sdb_id\":81490,\"quantity\":1},{\"item_guid\":2186143391660867840,\"item_sdb_id\":113687,\"quantity\":1},{\"item_guid\":2186270670333924352,\"item_sdb_id\":127743,\"quantity\":1},{\"item_guid\":2186143391660874496,\"item_sdb_id\":113718,\"quantity\":1},{\"item_guid\":745117697983358720,\"item_sdb_id\":131698,\"quantity\":1},{\"item_guid\":817027468102777856,\"item_sdb_id\":114074,\"quantity\":1},{\"item_guid\":817158353140239360,\"item_sdb_id\":139488,\"quantity\":1},{\"item_guid\":2186143391660858880,\"item_sdb_id\":136722,\"quantity\":1},{\"item_guid\":2186143391660875264,\"item_sdb_id\":113853,\"quantity\":1},{\"item_guid\":817158353140193280,\"item_sdb_id\":130002,\"quantity\":1},{\"item_guid\":2186143391660863744,\"item_sdb_id\":113552,\"quantity\":1},{\"item_guid\":1536551767489147136,\"item_sdb_id\":81490,\"quantity\":1},{\"item_guid\":817027462130273792,\"item_sdb_id\":114044,\"quantity\":1},{\"item_guid\":2186143391660864000,\"item_sdb_id\":114226,\"quantity\":1},{\"item_guid\":744965437836734720,\"item_sdb_id\":127097,\"quantity\":1},{\"item_guid\":2186265083587334656,\"item_sdb_id\":132178,\"quantity\":1},{\"item_guid\":2186143391660867072,\"item_sdb_id\":114316,\"quantity\":1},{\"item_guid\":2186143391660862976,\"item_sdb_id\":126539,\"quantity\":1},{\"item_guid\":817027462130281472,\"item_sdb_id\":129458,\"quantity\":1},{\"item_guid\":2258317732239097344,\"item_sdb_id\":128688,\"quantity\":1},{\"item_guid\":2186143391660865536,\"item_sdb_id\":128730,\"quantity\":1},{\"item_guid\":2258317732239093248,\"item_sdb_id\":136036,\"quantity\":1},{\"item_guid\":2186143391660870144,\"item_sdb_id\":126413,\"quantity\":1},{\"item_guid\":817027468102772992,\"item_sdb_id\":129020,\"quantity\":1},{\"item_guid\":2186143391660877568,\"item_sdb_id\":114074,\"quantity\":1},{\"item_guid\":744968734894291968,\"item_sdb_id\":140705,\"quantity\":1},{\"item_guid\":2186143391660873728,\"item_sdb_id\":126539,\"quantity\":1},{\"item_guid\":2186143391660862208,\"item_sdb_id\":129020,\"quantity\":1},{\"item_guid\":2258317732238628096,\"item_sdb_id\":143309,\"quantity\":1},{\"item_guid\":2186143391660869632,\"item_sdb_id\":136722,\"quantity\":1},{\"item_guid\":1536551767489147392,\"item_sdb_id\":81490,\"quantity\":1},{\"item_guid\":2186143391660878080,\"item_sdb_id\":113520,\"quantity\":1},{\"item_guid\":2258317732239018240,\"item_sdb_id\":131898,\"quantity\":1},{\"item_guid\":2186143391660872960,\"item_sdb_id\":129020,\"quantity\":1},{\"item_guid\":2186143391660861440,\"item_sdb_id\":129458,\"quantity\":1},{\"item_guid\":2186143391660864512,\"item_sdb_id\":113931,\"quantity\":1},{\"item_guid\":745116577399056896,\"item_sdb_id\":134794,\"quantity\":1},{\"item_guid\":2186143391660873472,\"item_sdb_id\":127270,\"quantity\":1},{\"item_guid\":2186143391660868864,\"item_sdb_id\":125725,\"quantity\":1},{\"item_guid\":2186143391660876288,\"item_sdb_id\":128604,\"quantity\":1},{\"item_guid\":745117697983348224,\"item_sdb_id\":132458,\"quantity\":1},{\"item_guid\":745116577399164928,\"item_sdb_id\":114008,\"quantity\":1},{\"item_guid\":817027462130251776,\"item_sdb_id\":128604,\"quantity\":1},{\"item_guid\":2186122473089611264,\"item_sdb_id\":143670,\"quantity\":1},{\"item_guid\":2186143391660868096,\"item_sdb_id\":113858,\"quantity\":1},{\"item_guid\":817027462130284288,\"item_sdb_id\":129020,\"quantity\":1},{\"item_guid\":2186265083587840256,\"item_sdb_id\":134832,\"quantity\":1},{\"item_guid\":745117697982909696,\"item_sdb_id\":135912,\"quantity\":1},{\"item_guid\":817027462130266624,\"item_sdb_id\":129458,\"quantity\":1},{\"item_guid\":2186143391660871424,\"item_sdb_id\":114160,\"quantity\":1},{\"item_guid\":2186143391660878848,\"item_sdb_id\":113838,\"quantity\":1},{\"item_guid\":744965437836734976,\"item_sdb_id\":127953,\"quantity\":1},{\"item_guid\":2186143391660867328,\"item_sdb_id\":114142,\"quantity\":1},{\"item_guid\":2186143391660874752,\"item_sdb_id\":114112,\"quantity\":1},{\"item_guid\":2329134320037442304,\"item_sdb_id\":77367,\"quantity\":1},{\"item_guid\":817027462130287360,\"item_sdb_id\":126539,\"quantity\":1},{\"item_guid\":2186265083587808256,\"item_sdb_id\":132068,\"quantity\":1},{\"item_guid\":2186143391660863232,\"item_sdb_id\":114044,\"quantity\":1},{\"item_guid\":744995722722969856,\"item_sdb_id\":141906,\"quantity\":1},{\"item_guid\":817044963318509568,\"item_sdb_id\":127223,\"quantity\":1},{\"item_guid\":2186143391660873216,\"item_sdb_id\":136576,\"quantity\":1},{\"item_guid\":817027462130271744,\"item_sdb_id\":127270,\"quantity\":1},{\"item_guid\":2186143391660870656,\"item_sdb_id\":114316,\"quantity\":1},{\"item_guid\":817027462130296320,\"item_sdb_id\":127874,\"quantity\":1},{\"item_guid\":817027468102731264,\"item_sdb_id\":127144,\"quantity\":1},{\"item_guid\":817027462130261760,\"item_sdb_id\":114094,\"quantity\":1},{\"item_guid\":744965437836734208,\"item_sdb_id\":125636,\"quantity\":1},{\"item_guid\":2186143391660866560,\"item_sdb_id\":126539,\"quantity\":1},{\"item_guid\":817027462130292224,\"item_sdb_id\":143437,\"quantity\":1},{\"item_guid\":2186143391660862464,\"item_sdb_id\":136576,\"quantity\":1},{\"item_guid\":817027468102746112,\"item_sdb_id\":128000,\"quantity\":1},{\"item_guid\":817034666132829184,\"item_sdb_id\":124671,\"quantity\":1},{\"item_guid\":817027468102775296,\"item_sdb_id\":127144,\"quantity\":1},{\"item_guid\":745116577399087616,\"item_sdb_id\":128689,\"quantity\":1},{\"item_guid\":2186143391660869888,\"item_sdb_id\":127144,\"quantity\":1},{\"item_guid\":2186143391660858368,\"item_sdb_id\":128604,\"quantity\":1},{\"item_guid\":744967187901213440,\"item_sdb_id\":130370,\"quantity\":1},{\"item_guid\":2186143391660868608,\"item_sdb_id\":129312,\"quantity\":1},{\"item_guid\":817027468102749440,\"item_sdb_id\":125683,\"quantity\":1},{\"item_guid\":2186270670332959744,\"item_sdb_id\":127013,\"quantity\":1},{\"item_guid\":2186143391660861696,\"item_sdb_id\":125683,\"quantity\":1},{\"item_guid\":817027462130298880,\"item_sdb_id\":125725,\"quantity\":1},{\"item_guid\":2186143391660857600,\"item_sdb_id\":127874,\"quantity\":1},{\"item_guid\":817027462130283264,\"item_sdb_id\":128730,\"quantity\":1},{\"item_guid\":2186143391660876544,\"item_sdb_id\":129020,\"quantity\":1},{\"item_guid\":817158353140205568,\"item_sdb_id\":135953,\"quantity\":1},{\"item_guid\":2186143391660865024,\"item_sdb_id\":129458,\"quantity\":1},{\"item_guid\":2186143391660872448,\"item_sdb_id\":125683,\"quantity\":1},{\"item_guid\":744961658868883968,\"item_sdb_id\":143123,\"quantity\":1},{\"item_guid\":2186143391660868352,\"item_sdb_id\":127874,\"quantity\":1},{\"item_guid\":744967187901204480,\"item_sdb_id\":143123,\"quantity\":1},{\"item_guid\":2186143391660866048,\"item_sdb_id\":136576,\"quantity\":1},{\"item_guid\":2186143391660875776,\"item_sdb_id\":129312,\"quantity\":1},{\"item_guid\":2186143391660871680,\"item_sdb_id\":113873,\"quantity\":1},{\"item_guid\":2186143391660859392,\"item_sdb_id\":126413,\"quantity\":1},{\"item_guid\":744965437836735232,\"item_sdb_id\":128557,\"quantity\":1},{\"item_guid\":817027468102774272,\"item_sdb_id\":136722,\"quantity\":1},{\"item_guid\":2186143391660875008,\"item_sdb_id\":113514,\"quantity\":1},{\"item_guid\":2186143391660870912,\"item_sdb_id\":114166,\"quantity\":1},{\"item_guid\":817158353140259072,\"item_sdb_id\":129272,\"quantity\":1},{\"item_guid\":2186265083587844608,\"item_sdb_id\":132110,\"quantity\":1},{\"item_guid\":744965437836734464,\"item_sdb_id\":126534,\"quantity\":1},{\"item_guid\":2186143391660878336,\"item_sdb_id\":113712,\"quantity\":1},{\"item_guid\":2186143391660866816,\"item_sdb_id\":113996,\"quantity\":1},{\"item_guid\":2258317732239108352,\"item_sdb_id\":132622,\"quantity\":1},{\"item_guid\":2186270670333924096,\"item_sdb_id\":132418,\"quantity\":1},{\"item_guid\":817158353140298752,\"item_sdb_id\":131939,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":142185,\"quantity\":1},{\"item_guid\":817027462130249728,\"item_sdb_id\":129312,\"quantity\":1},{\"item_guid\":1536551767489147904,\"item_sdb_id\":81490,\"quantity\":1},{\"item_guid\":2186143391660861952,\"item_sdb_id\":128730,\"quantity\":1},{\"item_guid\":817027462130253056,\"item_sdb_id\":129020,\"quantity\":1},{\"item_guid\":2186143391660869376,\"item_sdb_id\":129020,\"quantity\":1},{\"item_guid\":817027468102729984,\"item_sdb_id\":136722,\"quantity\":1},{\"item_guid\":2186143391660876800,\"item_sdb_id\":136722,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":0,\"quantity\":0},{\"item_guid\":817042546191304448,\"item_sdb_id\":129604,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":86696,\"quantity\":12},{\"item_guid\":0,\"item_sdb_id\":77014,\"quantity\":10},{\"item_guid\":0,\"item_sdb_id\":82597,\"quantity\":15},{\"item_guid\":0,\"item_sdb_id\":86709,\"quantity\":903},{\"item_guid\":0,\"item_sdb_id\":117036,\"quantity\":5},{\"item_guid\":0,\"item_sdb_id\":95086,\"quantity\":2},{\"item_guid\":0,\"item_sdb_id\":86669,\"quantity\":1487},{\"item_guid\":0,\"item_sdb_id\":75425,\"quantity\":470},{\"item_guid\":0,\"item_sdb_id\":77682,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":77015,\"quantity\":8},{\"item_guid\":0,\"item_sdb_id\":80274,\"quantity\":2},{\"item_guid\":0,\"item_sdb_id\":123386,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":85679,\"quantity\":7},{\"item_guid\":0,\"item_sdb_id\":82595,\"quantity\":108},{\"item_guid\":0,\"item_sdb_id\":82596,\"quantity\":128},{\"item_guid\":0,\"item_sdb_id\":82598,\"quantity\":174},{\"item_guid\":0,\"item_sdb_id\":77428,\"quantity\":335},{\"item_guid\":0,\"item_sdb_id\":77429,\"quantity\":330},{\"item_guid\":0,\"item_sdb_id\":82604,\"quantity\":20},{\"item_guid\":0,\"item_sdb_id\":77430,\"quantity\":3},{\"item_guid\":0,\"item_sdb_id\":123400,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":78032,\"quantity\":2},{\"item_guid\":0,\"item_sdb_id\":77643,\"quantity\":16},{\"item_guid\":0,\"item_sdb_id\":82624,\"quantity\":45},{\"item_guid\":0,\"item_sdb_id\":86672,\"quantity\":239},{\"item_guid\":0,\"item_sdb_id\":123217,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":86401,\"quantity\":10},{\"item_guid\":0,\"item_sdb_id\":56,\"quantity\":11},{\"item_guid\":0,\"item_sdb_id\":86667,\"quantity\":4978},{\"item_guid\":0,\"item_sdb_id\":95085,\"quantity\":17},{\"item_guid\":0,\"item_sdb_id\":86621,\"quantity\":11814},{\"item_guid\":0,\"item_sdb_id\":77607,\"quantity\":2},{\"item_guid\":0,\"item_sdb_id\":30298,\"quantity\":1272},{\"item_guid\":0,\"item_sdb_id\":77860,\"quantity\":2},{\"item_guid\":0,\"item_sdb_id\":124626,\"quantity\":26},{\"item_guid\":0,\"item_sdb_id\":139960,\"quantity\":6},{\"item_guid\":0,\"item_sdb_id\":139961,\"quantity\":12},{\"item_guid\":0,\"item_sdb_id\":54003,\"quantity\":4220},{\"item_guid\":0,\"item_sdb_id\":86679,\"quantity\":1344},{\"item_guid\":0,\"item_sdb_id\":77343,\"quantity\":14},{\"item_guid\":0,\"item_sdb_id\":81487,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":86668,\"quantity\":1560},{\"item_guid\":0,\"item_sdb_id\":86673,\"quantity\":608},{\"item_guid\":0,\"item_sdb_id\":86681,\"quantity\":13},{\"item_guid\":0,\"item_sdb_id\":86682,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":86683,\"quantity\":702},{\"item_guid\":0,\"item_sdb_id\":32755,\"quantity\":21},{\"item_guid\":0,\"item_sdb_id\":56835,\"quantity\":76},{\"item_guid\":0,\"item_sdb_id\":77344,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":77345,\"quantity\":6},{\"item_guid\":0,\"item_sdb_id\":76947,\"quantity\":10},{\"item_guid\":0,\"item_sdb_id\":86699,\"quantity\":5},{\"item_guid\":0,\"item_sdb_id\":77346,\"quantity\":14},{\"item_guid\":0,\"item_sdb_id\":86703,\"quantity\":10750},{\"item_guid\":0,\"item_sdb_id\":86711,\"quantity\":429},{\"item_guid\":0,\"item_sdb_id\":86713,\"quantity\":2140},{\"item_guid\":0,\"item_sdb_id\":116563,\"quantity\":36},{\"item_guid\":0,\"item_sdb_id\":77403,\"quantity\":7},{\"item_guid\":0,\"item_sdb_id\":124334,\"quantity\":11},{\"item_guid\":0,\"item_sdb_id\":140057,\"quantity\":110},{\"item_guid\":0,\"item_sdb_id\":95083,\"quantity\":9},{\"item_guid\":0,\"item_sdb_id\":95084,\"quantity\":33},{\"item_guid\":0,\"item_sdb_id\":85535,\"quantity\":21},{\"item_guid\":0,\"item_sdb_id\":76978,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":95087,\"quantity\":7},{\"item_guid\":0,\"item_sdb_id\":76979,\"quantity\":1},{\"item_guid\":0,\"item_sdb_id\":95088,\"quantity\":35},{\"item_guid\":0,\"item_sdb_id\":95089,\"quantity\":9},{\"item_guid\":0,\"item_sdb_id\":76984,\"quantity\":56},{\"item_guid\":0,\"item_sdb_id\":95093,\"quantity\":6},{\"item_guid\":0,\"item_sdb_id\":76985,\"quantity\":60},{\"item_guid\":0,\"item_sdb_id\":95094,\"quantity\":5},{\"item_guid\":0,\"item_sdb_id\":77404,\"quantity\":6},{\"item_guid\":0,\"item_sdb_id\":123353,\"quantity\":2},{\"item_guid\":0,\"item_sdb_id\":76986,\"quantity\":30},{\"item_guid\":0,\"item_sdb_id\":95095,\"quantity\":3},{\"item_guid\":0,\"item_sdb_id\":95096,\"quantity\":5},{\"item_guid\":0,\"item_sdb_id\":95097,\"quantity\":26},{\"item_guid\":0,\"item_sdb_id\":95098,\"quantity\":22},{\"item_guid\":0,\"item_sdb_id\":95099,\"quantity\":20},{\"item_guid\":0,\"item_sdb_id\":120974,\"quantity\":14},{\"item_guid\":0,\"item_sdb_id\":142268,\"quantity\":16},{\"item_guid\":0,\"item_sdb_id\":0,\"quantity\":0},{\"item_guid\":0,\"item_sdb_id\":124565,\"quantity\":11},{\"item_guid\":0,\"item_sdb_id\":82577,\"quantity\":547},{\"item_guid\":0,\"item_sdb_id\":77205,\"quantity\":316},{\"item_guid\":0,\"item_sdb_id\":77606,\"quantity\":3},{\"item_guid\":0,\"item_sdb_id\":0,\"quantity\":0},{\"item_guid\":0,\"item_sdb_id\":120622,\"quantity\":1}]},{\"definitions\":[],\"slots\":[]}]}"
        };

        client.NetChannels[ChannelType.ReliableGss].SendIAero(bagInventoryUpdate, player.CharacterEntity.EntityId);
    }

    [MessageID((byte)Commands.SetSteamUserId)]
    public void SetSteamUserId(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var setSteamIdPacket = packet.Unpack<SetSteamUserId>();
        player.SteamUserId = setSteamIdPacket.SteamUserId;
        _logger.Verbose("Entity {0:x8} Steam user id (Aero): {1}", entityId, player.SteamUserId);
        
        // var conventional = packet.Read<SetSteamIdRequest>();
        // _logger.Verbose("Packet Data: {0}", BitConverter.ToString(packet.PacketData.ToArray()).Replace("-", " "));
        // _logger.Verbose("Entity {0:x8} Steam user id (conventional): {1}", entityId, conventional.SteamId);
    }

    [MessageID((byte)Commands.VehicleCalldownRequest)]
    public void VehicleCalldownRequest(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        Core.Data.EntityGuid vehicleEntityGuid = new(31, client.AssignedShard.CurrentTime, 3670903, 0x01);

        var vehicleCalldownRequest = packet.Unpack<VehicleCalldownRequest>();

        if (vehicleCalldownRequest == null)
        {
            return;
        }

        var vehicleBaseController = new VController.BaseController
        {
            VehicleIdProp = vehicleCalldownRequest.VehicleID,
            ConfigurationProp = new ConfigurationData { Data = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0 } },
            FlagsProp = new byte[] { 0x41, 0x41, 0x51, 0x41, 0x41, 0x41, 0x3d, 0x3d },
            EngineStateProp = 0,
            PathStateProp = 1,
            OwnerIdProp = new EntityId { Backing = vehicleEntityGuid.Full, ControllerId = Controller.Vehicle, Id = player.PlayerId },
            OwnerNameProp = string.Empty,
            OwnerLocalStringProp = 0,
            OccupantIds_0Prop = new EntityId { Backing = 0, ControllerId = 0, Id = 0 },
            OccupantIds_1Prop = new EntityId { Backing = 0, ControllerId = 0, Id = 0 },
            OccupantIds_2Prop = new EntityId { Backing = 0, ControllerId = 0, Id = 0 },
            OccupantIds_3Prop = new EntityId { Backing = 0, ControllerId = 0, Id = 0 },
            OccupantIds_4Prop = new EntityId { Backing = 0, ControllerId = 0, Id = 0 },
            OccupantIds_5Prop = new EntityId { Backing = 0, ControllerId = 0, Id = 0 },
            DeployableIds_0Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 255 },
            DeployableIds_1Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 255 },
            DeployableIds_2Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 255 },
            DeployableIds_3Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 255 },
            DeployableIds_4Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 255 },
            DeployableIds_5Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 255 },
            DeployableIds_6Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 255 },
            DeployableIds_7Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 255 },
            DeployableIds_8Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 255 },
            DeployableIds_9Prop = new DeployableIdsData { Target = new EntityId { Backing = 0, ControllerId = 0, Id = 0 }, Unk1 = 0, Unk2 = 255 },
            SnapMountProp = 0,
            SpawnPoseProp = new VController.SpawnPoseData
                            {
                                Position = vehicleCalldownRequest.Position,
                                Rotation = vehicleCalldownRequest.Rotation,
                                Direction = player.CharacterEntity.AimDirection,
                                Time = client.AssignedShard.CurrentTime
                            },
            SpawnVelocityProp = new Vector3 { X = 0, Y = 0, Z = 0 },
            CurrentPoseProp = new CurrentPoseData
                              {
                                  Position = vehicleCalldownRequest.Position,
                                  Rotation = vehicleCalldownRequest.Rotation,
                                  Direction = player.CharacterEntity.AimDirection,
                                  State = 4096, // What state might this be?
                                  Time = client.AssignedShard.CurrentTime
                              },
            ProcessDelayProp = new ProcessDelayData { Unk1 = 15734, Unk2 = 300 },
            HostilityInfoProp = new HostilityInfoData
                                {
                                    Flags = HostilityInfoData.HostilityFlags.Faction,
                                    FactionId = 1,
                                    TeamId = 0,
                                    Unk2 = 0,
                                    Unk3 = 0,
                                    Unk4 = 0
                                },
            PersonalFactionStanceProp = new PersonalFactionStanceData
                                        {
                                            Unk1 = new PersonalFactionStanceBitfield { NumFactions = 50, Bitfield = new byte[] { 0x43, 0x51, 0x35, 0x64, 0x2f, 0x31, 0x38, 0x49, 0x41, 0x41, 0x41, 0x3d } },
                                            Unk2 = new PersonalFactionStanceBitfield { NumFactions = 50, Bitfield = new byte[] { 0x38, 0x67, 0x41, 0x67, 0x41, 0x41, 0x44, 0x79, 0x41, 0x41, 0x41, 0x3d } }
                                        },
            CurrentHealthProp = 60684,
            MaxHealthProp = 60684,
            CurrentShieldsProp = 0,
            MaxShieldsProp = 0,
            CurrentResourcesProp = 0,
            MaxResourcesProp = 0,
            WaterLevelAndDescProp = 0,
            SinFlagsProp = 0,
            SinFlagsPrivateProp = 0,
            SinFactionsAcquiredByProp = null,
            SinTeamsAcquiredByProp = null,
            SinCardTypeProp = 0,
            SinCardFields_0Prop = null,
            SinCardFields_1Prop = null,
            SinCardFields_2Prop = null,
            SinCardFields_3Prop = null,
            SinCardFields_4Prop = null,
            SinCardFields_5Prop = null,
            SinCardFields_6Prop = null,
            SinCardFields_7Prop = null,
            SinCardFields_8Prop = null,
            SinCardFields_9Prop = null,
            SinCardFields_10Prop = null,
            SinCardFields_11Prop = null,
            SinCardFields_12Prop = null,
            SinCardFields_13Prop = null,
            SinCardFields_14Prop = null,
            SinCardFields_15Prop = null,
            SinCardFields_16Prop = null,
            SinCardFields_17Prop = null,
            SinCardFields_18Prop = null,
            SinCardFields_19Prop = null,
            SinCardFields_20Prop = null,
            SinCardFields_21Prop = null,
            SinCardFields_22Prop = null,
            ScopeBubbleInfoProp = new ScopeBubbleInfoData { Unk1 = 0, Unk2 = 1 },
            ScalingLevelProp = 0
        };

        var vehicleCombatController = new VController.CombatController
        {
            SlottedAbility_0 = 0,
            SlottedAbility_1 = 0,
            SlottedAbility_2 = 0,
            SlottedAbility_3 = 0,
            SlottedAbility_4 = 0,
            SlottedAbility_5 = 34920,
            SlottedAbility_6 = 0,
            SlottedAbility_7 = 0,
            SlottedAbility_8 = 43,

            SlottedAbility_0Prop = 0,
            SlottedAbility_1Prop = 0,
            SlottedAbility_2Prop = 0,
            SlottedAbility_3Prop = 0,
            SlottedAbility_4Prop = 0,
            SlottedAbility_5Prop = 34920,
            SlottedAbility_6Prop = 0,
            SlottedAbility_7Prop = 0,
            SlottedAbility_8Prop = 43,

            StatusEffectsChangeTime_0Prop = 11465,
            StatusEffectsChangeTime_1Prop = 3139,
            StatusEffectsChangeTime_2Prop = 2132,
            StatusEffectsChangeTime_3Prop = 5763,
            StatusEffectsChangeTime_4Prop = 29801,
            StatusEffectsChangeTime_5Prop = 28521,
            StatusEffectsChangeTime_6Prop = 8302,
            StatusEffectsChangeTime_7Prop = 25970,
            StatusEffectsChangeTime_8Prop = 27760,
            StatusEffectsChangeTime_9Prop = 25441,
            StatusEffectsChangeTime_10Prop = 25701,
            StatusEffectsChangeTime_11Prop = 24864,
            StatusEffectsChangeTime_12Prop = 8308,
            StatusEffectsChangeTime_13Prop = 27749,
            StatusEffectsChangeTime_14Prop = 28005,
            StatusEffectsChangeTime_15Prop = 28261,
            StatusEffectsChangeTime_16Prop = 8308,
            StatusEffectsChangeTime_17Prop = 2609,
            StatusEffectsChangeTime_18Prop = 14641,
            StatusEffectsChangeTime_19Prop = 12602,
            StatusEffectsChangeTime_20Prop = 14902,
            StatusEffectsChangeTime_21Prop = 14645,
            StatusEffectsChangeTime_22Prop = 20000,
            StatusEffectsChangeTime_23Prop = 30319,
            StatusEffectsChangeTime_24Prop = 12576,
            StatusEffectsChangeTime_25Prop = 8244,
            StatusEffectsChangeTime_26Prop = 12338,
            StatusEffectsChangeTime_27Prop = 13873,
            StatusEffectsChangeTime_28Prop = 11552,
            StatusEffectsChangeTime_29Prop = 28704,
            StatusEffectsChangeTime_30Prop = 29551,
            StatusEffectsChangeTime_31Prop = 29801,

            StatusEffects_0Prop = null,
            StatusEffects_1Prop = null,
            StatusEffects_2Prop = null,
            StatusEffects_3Prop = null,
            StatusEffects_4Prop = null,
            StatusEffects_5Prop = null,
            StatusEffects_6Prop = null,
            StatusEffects_7Prop = null,
            StatusEffects_8Prop = null,
            StatusEffects_9Prop = null,
            StatusEffects_10Prop = null,
            StatusEffects_11Prop = null,
            StatusEffects_12Prop = null,
            StatusEffects_13Prop = null,
            StatusEffects_14Prop = null,
            StatusEffects_15Prop = null,
            StatusEffects_16Prop = null,
            StatusEffects_17Prop = null,
            StatusEffects_18Prop = null,
            StatusEffects_19Prop = null,
            StatusEffects_20Prop = null,
            StatusEffects_21Prop = null,
            StatusEffects_22Prop = null,
            StatusEffects_23Prop = null,
            StatusEffects_24Prop = null,
            StatusEffects_25Prop = null,
            StatusEffects_26Prop = null,
            StatusEffects_27Prop = null,
            StatusEffects_28Prop = null,
            StatusEffects_29Prop = null,
            StatusEffects_30Prop = null,
            StatusEffects_31Prop = null
        };

        client.NetChannels[ChannelType.ReliableGss].SendIAeroControllerKeyframe(vehicleBaseController, vehicleEntityGuid.Full, player.PlayerId);
        client.NetChannels[ChannelType.ReliableGss].SendIAeroControllerKeyframe(vehicleCombatController, vehicleEntityGuid.Full, player.PlayerId);
    }

    [MessageID((byte)Commands.SetEffectsFlag)]
    public void SetEffectsFlag(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<SetEffectsFlag>();
        player.CharacterEntity.SetEffectsFlags(query.Flashlight);
    }

    [MessageID((byte)Commands.PerformEmote)]
    public void PerformEmote(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<PerformEmote>();
        player.CharacterEntity.SetEmote(new EmoteData { Id = query.EmoteId, Time = query.Time });
    }

    [MessageID((byte)Commands.ClientQueryInteractionStatus)]
    public void ClientQueryInteractionStatus(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var query = packet.Unpack<ClientQueryInteractionStatus>();
        ulong requestedEntityId = query.Entity.Id;
        var found = client.AssignedShard.Entities.TryGetValue(requestedEntityId, out var entity);
        if (found)
        {
            if (entity.IsInteractable())
            {
                // TODO: Support interactable entities, get interaction data and send AddOrUpdateInteractives 
            }
            else
            {
                var response = new RemoveInteractives
                {
                    Entities = new EntityId[] { new EntityId { Backing = requestedEntityId } }
                };
                client.NetChannels[ChannelType.ReliableGss].SendIAero(response, player.CharacterEntity.EntityId);
            }
        }
        else
        {
            _logger.Verbose("ClientQueryInteractionStatus entity {0:x8} not found!", requestedEntityId);
        }
    }

    [MessageID((byte)Commands.ResourceLocationInfosRequest)]
    public void ResourceLocationInfosRequest(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var resourceLocationInfosResponse = new ResourceLocationInfosResponse
        {
            Data = new ResourceLocationInfo[] { },
            Unk = 0x01
        };

        client.NetChannels[ChannelType.ReliableGss].SendIAero(resourceLocationInfosResponse, player.CharacterEntity.EntityId);
    }

    [MessageID((byte)Commands.FriendsListRequest)]
    public void FriendsListRequest(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var friendsListResponse = new FriendsListResponse
        {
            Unk1 = new FriendsListData[]
            {
                new FriendsListData
                {
                    Unk1 = 9162788533740412926,
                    Unk2 = "TestUser1",
                    Unk3 = string.Empty,
                    Unk4 = 1,
                    Unk5 = 1427570048,
                    Unk6 = 1
                },
                new FriendsListData
                {
                    Unk1 = 9153042507174448638,
                    Unk2 = "TestUser2",
                    Unk3 = string.Empty,
                    Unk4 = 1,
                    Unk5 = 1471686583,
                    Unk6 = 1
                }
            },
            Unk2 = 0
        };

        client.NetChannels[ChannelType.ReliableGss].SendIAero(friendsListResponse, player.CharacterEntity.EntityId);
    }

    [MessageID((byte)Commands.MapOpened)]
    public void MapOpened(INetworkClient client, IPlayer player, ulong entityId, GamePacket packet)
    {
        var mapOpened = new GeographicalReportResponse
        {
            ScanId = 0,
            Position = new Vector3 { X = 0, Y = 0, Z = 0 },
            Valid = 0x00,
            Composition = new ResourceCompositionData[] { }
        };

        client.NetChannels[ChannelType.ReliableGss].SendIAero(mapOpened, player.CharacterEntity.EntityId);
    }
}