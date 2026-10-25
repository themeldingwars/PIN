using AeroMessages.GSS.Character;
using AeroMessages.Matrix;
using GameServer.Data;
using GameServer.Entities.Character;
using GameServer.Protocol;
using GameServer.Systems.SystemEvents;
using GameServer.Test;
using Serilog;
using ClientStatus = GameServer.Protocol.ClientStatus;

namespace GameServer.Systems.CharacterCreation;

public class CharacterCreationService
{
    private readonly IShard _shard;
    private readonly IEventBus _eventBus;
    private readonly ICharacterCreationRules _rules;
    private readonly ILogger _logger;

    public CharacterCreationService(IShard shard, IEventBus eventBus, ICharacterCreationRules rules)
    {
        _shard = shard;
        _eventBus = eventBus;
        _rules = rules;
        _logger = shard.Logger.ForContext<CharacterCreationService>();

        _eventBus.Subscribe<CharacterDataFetchedEvent>(OnCharacterDataFetched);
    }

    private void OnCharacterDataFetched(CharacterDataFetchedEvent evt)
    {
        var client = evt.Client;
        var player = client.Player;
        var data = evt.Data;
        var guid = player.CharacterId;

        // The client may have dropped, or moved on, while the character data was being fetched
        if (client.NetClientStatus is not (ClientStatus.Connected or ClientStatus.Idle) || !_shard.Clients.ContainsKey(client.SocketId))
        {
            _logger.Information("Discarding fetched character data for socket {SocketId} — the client is no longer on this shard.", client.SocketId);
            return;
        }

        player.PlayerId = 0x4658281c142e9f00ul;

        player.CharacterEntity = new CharacterEntity(_shard, guid);

        player.Inventory = new CharacterInventory(_shard, client, player.CharacterEntity);
        if (!player.Inventory.LoadRemote(data.Inventory))
        {
            player.Inventory.LoadHardcodedInventory();
        }

        player.Unlocks = new CharacterUnlocks(client, player.CharacterEntity, guid + (byte)GuidService.AdditionalTypes.Character);
        player.Unlocks.LoadRemote(data.Unlocks);
        player.Battleframes = new CharacterBattleframes(guid + (byte)GuidService.AdditionalTypes.Character);
        player.Battleframes.LoadRemote(data.Battleframes);
        player.Inventory.ApplyBattleframeBandwidth(player.Battleframes);

        int loadoutId;
        if (data.Character != null)
        {
            player.CharacterEntity.LoadRemote(data.Character);

            loadoutId = player.Inventory.GetLoadoutIdForChassis(data.Character.CharacterInfo.CurrentBattleframeSDBId);
            if (loadoutId == 0)
            {
                loadoutId = player.Inventory.GetAnyLoadoutId();
            }
        }
        else
        {
            player.CharacterEntity.Load(HardcodedCharacterData.FallbackData);
            loadoutId = player.Inventory.GetLoadoutIdForChassis(_rules.FallbackChassisId);
        }

        var loadoutRefData = player.Inventory.GetLoadoutReferenceData(loadoutId);
        var loadout = new CharacterLoadout(loadoutRefData);
        _shard.Admin.ApplyEquipmentOverrides((INetworkPlayer)player, loadout);
        player.Battleframes.ApplyFrameProgression(player.CharacterEntity, loadout.ChassisID);
        player.CharacterEntity.ApplyLoadout(loadout);

        // Collision data should have been initialized now, create physics representation
        _shard.Physics.CreateKineticEntity(player.CharacterEntity);

        player.CharacterEntity.SetControllingPlayer((INetworkPlayer)player);
        player.CharacterEntity.CanBleedout = true;
        _shard.CharacterLifecycle.OnCharacterCreated(player.CharacterEntity);
        player.CharacterEntity.SetCharacterState(CharacterStateData.CharacterStatus.Spawning, _shard.CurrentTime);
        player.Status = IPlayer.PlayerStatus.LoggedIn;

        _rules.OnCharacterCreated((INetworkPlayer)player, player.CharacterEntity);

        var wel = new WelcomeToTheMatrix { PlayerID = player.PlayerId, Unk1 = [], Unk2 = [] };
        client.NetChannels[ChannelType.Matrix].SendMessage(wel);

        var zone = DataUtils.GetZone(_shard.ZoneId);

        uint outpostId;
        if (data.Character != null)
        {
            outpostId = data.Character.CharacterInfo.LastZoneId == zone.ID
                            ? player.FindClosestAvailableOutpost(zone, data.Character.CharacterInfo.LastOutpostId)
                            : zone.DefaultOutpostId;
        }
        else
        {
            outpostId = zone.DefaultOutpostId;
        }

        _logger.Information("Zone {ZoneId} Outpost {OutpostId}", zone.ID, outpostId);

        player.EnterZone(zone, outpostId);
    }
}
