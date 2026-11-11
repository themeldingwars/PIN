using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AeroMessages.GSS;
using AeroMessages.GSS.Character;
using AeroMessages.GSS.Character.Command;
using AeroMessages.GSS.Character.Event;
using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
using GameServer.Protocol;
using GameServer.StaticDB;
using GameServer.Systems.CharacterLifecycle;
using GameServer.Systems.SystemEvents;
using Serilog;

namespace GameServer.Systems.Duels;

public class DuelService
{
    // DuelData.WinningTeam values
    private const sbyte _incomplete = -1;
    private const sbyte _draw = 0;
    private const sbyte _initiatorTeam = 1;
    private const sbyte _recipientTeam = 2;

    private static readonly ILogger _logger = Log.ForContext<DuelService>();

    private readonly IShard _shard;
    private readonly IDuelRules _rules;

    private readonly Dictionary<ulong, Duel> _duels = [];

    private ulong _lastDuelId;
    private ushort _lastTeamId;

    public DuelService(IShard shard, IEventBus eventBus, IDuelRules rules)
    {
        _shard = shard;
        _rules = rules;

        eventBus.Subscribe<CharacterEnteredBleedoutEvent>(evt => OnDowned(evt.Target));
        eventBus.Subscribe<CharacterDiedEvent>(evt => OnDowned(evt.Target));
    }

    public void HandleRequest(INetworkPlayer player, DuelRequest request)
    {
        if (player.CharacterEntity == null)
        {
            return;
        }

        switch (request.RequestedState)
        {
            case DuelState.Proposed:
                Propose(player, request.Duel.RecipientName);
                break;
            case DuelState.Confirmed:
                Answer(player, request.Duel, accept: true);
                break;
            case DuelState.Cancelled:
                Answer(player, request.Duel, accept: false);
                break;
            default:
                _logger.Warning("{Player} asked to move a duel to {State}", player.CharacterEntity, request.RequestedState);
                break;
        }
    }

    public void OnCharacterRemoved(CharacterEntity character)
    {
        if (!_duels.TryGetValue(character.EntityId, out var duel))
        {
            return;
        }

        if (duel.State == DuelState.Proposed)
        {
            Cancel(duel);
        }
        else
        {
            Complete(duel, _incomplete);
        }
    }

    private static HostilityInfoData WithDuelTeams(HostilityInfoData info, ushort ownTeam, ushort opposingTeam)
    {
        info.Flags |= HostilityInfoData.HostilityFlags.Unk2 | HostilityInfoData.HostilityFlags.Unk3;
        info.Unk2 = ownTeam;
        info.Unk3 = opposingTeam;
        return info;
    }

    private void Propose(INetworkPlayer initiator, string recipientName)
    {
        var character = initiator.CharacterEntity;

        if (_duels.TryGetValue(character.EntityId, out var pending))
        {
            // "/duel <name>" back at the player who challenged you accepts the challenge
            if (pending.State == DuelState.Proposed
                && pending.RecipientPlayer == initiator
                && string.Equals(pending.InitiatorName, recipientName?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                Accept(pending);
                return;
            }

            initiator.SendDebugChat("You already have a duel going");
            return;
        }

        if (_shard.CharacterLifecycle.GetState(character) != CharacterLifecycleState.Living)
        {
            return;
        }

        var recipient = FindPlayer(recipientName);
        if (recipient == null)
        {
            initiator.SendDebugChat($"There is no {recipientName} here to duel");
            return;
        }

        if (recipient == initiator)
        {
            return;
        }

        if (_duels.ContainsKey(recipient.CharacterEntity.EntityId))
        {
            initiator.SendDebugChat($"{recipient.CharacterEntity.StaticInfo.DisplayName} is already in a duel");
            return;
        }

        var duel = new Duel(++_lastDuelId, initiator, recipient)
        {
            InitiatorTeam = NextTeamId(),
            RecipientTeam = NextTeamId(),
        };
        _duels[duel.Initiator.EntityId] = duel;
        _duels[duel.Recipient.EntityId] = duel;

        _logger.Information("{Initiator} challenged {Recipient} to duel {DuelId}", duel.Initiator, duel.Recipient, duel.Id);

        PlantFlag(duel);
        Send(duel);

        _shard.Post(
            () =>
            {
                if (!duel.Ended && duel.State == DuelState.Proposed)
                {
                    Cancel(duel);
                }
            },
            _rules.ProposalTimeoutMs);
    }

    private void Answer(INetworkPlayer recipient, DuelData proposal, bool accept)
    {
        if (!_duels.TryGetValue(recipient.CharacterEntity.EntityId, out var duel)
            || duel.State != DuelState.Proposed
            || duel.RecipientPlayer != recipient
            || duel.Initiator.AeroEntityId.Backing != proposal.InitiatorId.Backing)
        {
            _logger.Debug("{Player} answered a duel proposal from {Initiator} that isn't open", recipient.CharacterEntity, proposal.InitiatorName);
            return;
        }

        if (!accept)
        {
            duel.InitiatorPlayer.SendDebugChat($"{duel.RecipientName} declined the duel");
            Cancel(duel);
            return;
        }

        Accept(duel);
    }

    private void Accept(Duel duel)
    {
        duel.State = DuelState.Confirmed;
        duel.StartTime = _shard.CurrentTime + _rules.CountdownMs;

        _logger.Information("{Recipient} accepted duel {DuelId} with {Initiator}", duel.Recipient, duel.Id, duel.Initiator);

        Send(duel);

        _shard.Post(() => Start(duel), _rules.CountdownMs);
    }

    private void Start(Duel duel)
    {
        if (duel.Ended)
        {
            return;
        }

        if (_shard.CharacterLifecycle.GetState(duel.Initiator) != CharacterLifecycleState.Living
            || _shard.CharacterLifecycle.GetState(duel.Recipient) != CharacterLifecycleState.Living)
        {
            Complete(duel, _incomplete);
            return;
        }

        duel.InitiatorHostility = duel.Initiator.HostilityInfo;
        duel.RecipientHostility = duel.Recipient.HostilityInfo;
        duel.Initiator.SetHostilityInfo(WithDuelTeams(duel.Initiator.HostilityInfo, duel.InitiatorTeam, duel.RecipientTeam));
        duel.Recipient.SetHostilityInfo(WithDuelTeams(duel.Recipient.HostilityInfo, duel.RecipientTeam, duel.InitiatorTeam));

        // Not sent, the client has no handler for Active
        duel.State = DuelState.Active;

        _logger.Information("Duel {DuelId} between {Initiator} and {Recipient} started", duel.Id, duel.Initiator, duel.Recipient);

        _shard.Post(
            () =>
            {
                if (!duel.Ended && duel.State == DuelState.Active)
                {
                    Complete(duel, _draw);
                }
            },
            _rules.TimeLimitMs);
    }

    private void OnDowned(CharacterEntity character)
    {
        if (!_duels.TryGetValue(character.EntityId, out var duel))
        {
            return;
        }

        switch (duel.State)
        {
            case DuelState.Active:
                Complete(duel, character == duel.Initiator ? _recipientTeam : _initiatorTeam);
                break;
            case DuelState.Confirmed:
                Complete(duel, _incomplete);
                break;
        }
    }

    private void Cancel(Duel duel)
    {
        duel.State = DuelState.Cancelled;
        Send(duel);
        End(duel);
    }

    private void Complete(Duel duel, sbyte winningTeam)
    {
        if (duel.State == DuelState.Active)
        {
            // Cleared as 0/0, left off the wire the client keeps the old pair
            duel.Initiator.SetHostilityInfo(WithDuelTeams(duel.InitiatorHostility, 0, 0));
            duel.Recipient.SetHostilityInfo(WithDuelTeams(duel.RecipientHostility, 0, 0));
        }

        duel.State = DuelState.Completed;
        duel.WinningTeam = winningTeam;

        _logger.Information(
            "Duel {DuelId} between {Initiator} and {Recipient} ended, winning team {WinningTeam}",
            duel.Id,
            duel.Initiator,
            duel.Recipient,
            winningTeam);

        Send(duel);
        End(duel);

        if (winningTeam == _initiatorTeam)
        {
            _shard.Chat.SendDuelResult(duel.InitiatorName, duel.RecipientName);
        }
        else if (winningTeam == _recipientTeam)
        {
            _shard.Chat.SendDuelResult(duel.RecipientName, duel.InitiatorName);
        }

        foreach (var character in new[] { duel.Initiator, duel.Recipient })
        {
            _shard.Post(() => RestoreAfterDuel(character), _rules.ReviveDelayMs);
        }
    }

    private void PlantFlag(Duel duel)
    {
        if (_rules.FlagDeployableId == 0 || SDBInterface.GetDeployable(_rules.FlagDeployableId) == null)
        {
            return;
        }

        var towards = duel.Recipient.Position - duel.Initiator.Position;
        towards.Z = 0;
        var position = duel.Initiator.Position;
        if (towards.LengthSquared() > 0.01f)
        {
            position += Vector3.Normalize(towards) * _rules.FlagDistance;
        }

        if (_shard.Physics.TryFindSurfaceSpawn(position + (Vector3.UnitZ * 2f), -Vector3.UnitZ, 4f, 0f, 0f, 0, out var grounded))
        {
            position = grounded;
        }

        duel.Flag = _shard.EntityMan.SpawnDeployable(_rules.FlagDeployableId, position, duel.Initiator.Orientation, owner: duel.Initiator);
        duel.Flag.SetHostilityInfo(WithDuelTeams(duel.Flag.HostilityInfo, duel.InitiatorTeam, duel.RecipientTeam));
    }

    private void End(Duel duel)
    {
        duel.Ended = true;

        if (duel.Flag != null)
        {
            _shard.EntityMan.Remove(duel.Flag);
            duel.Flag = null;
        }

        foreach (var entityId in new[] { duel.Initiator.EntityId, duel.Recipient.EntityId })
        {
            if (_duels.TryGetValue(entityId, out var current) && current == duel)
            {
                _duels.Remove(entityId);
            }
        }
    }

    private void RestoreAfterDuel(CharacterEntity character)
    {
        if (!_shard.Entities.ContainsKey(character.EntityId))
        {
            return;
        }

        if (_shard.CharacterLifecycle.GetState(character) == CharacterLifecycleState.Bleedout)
        {
            _shard.CharacterLifecycle.TryRevive(character);
        }

        if (_shard.CharacterLifecycle.GetState(character) != CharacterLifecycleState.Living)
        {
            return;
        }

        character.SetCurrentHealth(character.MaxHealth.Value);
        character.SetCurrentShields(character.MaxShields.Value);
    }

    private void Send(Duel duel)
    {
        var notification = new DuelNotification
        {
            Duel = new DuelData
            {
                State = duel.State,
                Reason = string.Empty,
                InitiatorId = duel.Initiator.AeroEntityId,
                InitiatorName = duel.InitiatorName,
                RecipientId = duel.Recipient.AeroEntityId,
                RecipientName = duel.RecipientName,
                Team1 = [duel.Initiator.AeroEntityId.Backing],
                Team2 = [duel.Recipient.AeroEntityId.Backing],
                StartTime = duel.StartTime,
                WinningTeam = duel.WinningTeam,
            },
            Unk2 = duel.Id,
        };

        foreach (var player in new[] { duel.InitiatorPlayer, duel.RecipientPlayer })
        {
            if (player.CanReceiveGSS && player.CharacterEntity != null)
            {
                player.NetChannels[ChannelType.ReliableGss].SendMessage(notification, player.CharacterEntity.EntityId);
            }
        }
    }

    private INetworkPlayer FindPlayer(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return _shard.Clients.Values.FirstOrDefault(player =>
            player.Status == IPlayer.PlayerStatus.Playing
            && player.CharacterEntity is { IsPlayerControlled: true } character
            && string.Equals(character.StaticInfo.DisplayName, name.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private ushort NextTeamId()
    {
        // 0 means no duel team
        _lastTeamId = _lastTeamId == ushort.MaxValue ? (ushort)1 : (ushort)(_lastTeamId + 1);
        return _lastTeamId;
    }

    private sealed class Duel
    {
        public Duel(ulong id, INetworkPlayer initiator, INetworkPlayer recipient)
        {
            Id = id;
            InitiatorPlayer = initiator;
            RecipientPlayer = recipient;
            Initiator = initiator.CharacterEntity;
            Recipient = recipient.CharacterEntity;
            InitiatorName = Initiator.StaticInfo.DisplayName;
            RecipientName = Recipient.StaticInfo.DisplayName;
        }

        public ulong Id { get; }
        public INetworkPlayer InitiatorPlayer { get; }
        public INetworkPlayer RecipientPlayer { get; }
        public CharacterEntity Initiator { get; }
        public CharacterEntity Recipient { get; }
        public string InitiatorName { get; }
        public string RecipientName { get; }
        public ushort InitiatorTeam { get; init; }
        public ushort RecipientTeam { get; init; }
        public DuelState State { get; set; } = DuelState.Proposed;
        public uint StartTime { get; set; }
        public sbyte WinningTeam { get; set; } = _incomplete;
        public HostilityInfoData InitiatorHostility { get; set; }
        public HostilityInfoData RecipientHostility { get; set; }
        public bool Ended { get; set; }
        public DeployableEntity Flag { get; set; }
    }
}
