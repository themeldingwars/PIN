using System;
using System.Collections.Generic;
using System.Numerics;
using AeroMessages.Common;
using AeroMessages.GSS;
using AeroMessages.GSS.AreaVisualData;
using AeroMessages.GSS.Character;
using AeroMessages.GSS.Character.Controller;
using AeroMessages.GSS.Character.Event;
using AeroMessages.GSS.Generic.Event.EncounterView;
using GameServer.Entities;
using GameServer.Entities.AreaVisualData;
using GameServer.Entities.Deployable;
using GameServer.Entities.Vehicle;
using GameServer.GRPC;
using GameServer.Protocol;
using GameServer.StaticDB.Records.customdata.Encounters;
using GameServer.Systems.Aptitude;
using GrpcGameServerAPIClient;
using Command = GrpcGameServerAPIClient.Command;
using SinCardTimer = AeroMessages.GSS.Timer;
using VehicleForcedMovement = AeroMessages.GSS.Vehicle.Event.ForcedMovement;

namespace GameServer.Systems.Encounters.Encounters;

public class KisutonLgvChallenge : BaseEncounter, IExitAttachmentHandler, IProximityHandler, ICanTimeout
{
    public const uint TerminalTypeId = 2499;
    private const ushort _terromotoTurboCobra = 132;
    private const uint _invisibleDeployable = 559;
    private const uint _currentMarkerEffect = 8444;
    private const uint _nextMarkerEffect = 8471;
    private const uint _finishLinePfx = 308460;
    private const float _finishLineHeight = 8;
    private const float _finishLineScale = 4f;
    private const ushort _mapMarkerRace = 26;
    private const uint _kisutonVoucher = 118354;
    private const uint _rewardExperience = 946;
    private const uint _rewardVouchers = 13;
    private const uint _goldBonusVouchers = 3;
    private const ulong _goldTimeMs = 87_000;
    private const uint _goldFinishTitle = 417177;
    private const uint _silverFinishTitle = 438150;
    private const uint _proximityDistance = 4;
    private const float _markerScopeRange = 500;
    private const ulong _countdownMs = 6_000;
    private const ulong _objectiveTimeMs = 30_000;
    private const ulong _waypointBonusTimeMs = 2_000;
    private const ulong _immobilizeDelayMs = 500;
    private const ulong _markerDeliveryDelayMs = 1_000;
    private const uint _markerSlideMs = 600;

    private readonly List<Vector3> _waypoints;
    private readonly ushort _laps;
    private readonly uint _leaderboardId;
    private readonly VehicleEntity _vehicle;
    private readonly AreaVisualDataEntity _checkpoint;
    private DeployableEntity _currentMarker;
    private DeployableEntity _nextMarker;
    private AreaVisualDataEntity _finishMarker;
    private Vector3 _currentMarkerTarget;
    private Vector3 _nextMarkerTarget;
    private PersonalMapMarkerData _mapMarker;
    private int _waypointIndex;
    private ulong _raceStartTime;
    private ulong _expireAt;

    public KisutonLgvChallenge(IShard shard, ulong entityId, INetworkPlayer soloParticipant, KisutonLgvChallengeDef data)
        : base(shard, entityId, soloParticipant)
    {
        _waypoints = data.Waypoints;
        _laps = Math.Max(data.Laps, (ushort)1);
        _leaderboardId = data.LeaderboardId;

        _vehicle = Shard.EntityMan.SpawnVehicle(
            _terromotoTurboCobra,
            data.StartPosition.Position,
            data.StartPosition.Orientation,
            SoloParticipant.CharacterEntity,
            true);
        _vehicle.Encounter = new EncounterComponent()
            {
                EncounterId = entityId, Instance = this, Events = EncounterComponent.Event.ExitAttachment
            };

        View = new RaceView()
               {
                   waypoint_goalProp = (ushort)_waypoints.Count,
                   objective_timerProp = new SinCardTimer()
                   {
                       Micro = (Shard.CurrentTimeLong + _countdownMs) * 1000,
                       State = SinCardTimer.TimerState.CountingDown,
                   },
                   race_startedProp = 0,
                   challenge_nameProp = data.ChallengeName,
                   lap_counterProp = 0,
                   lap_goalProp = _laps,
                   race_typeProp = data.Type,
                   track_nameProp = data.TrackName,
                   waypoint_counterProp = 0,
               };

        _currentMarkerTarget = _waypoints[_waypointIndex];
        _nextMarkerTarget = _waypoints[(_waypointIndex + 1) % _waypoints.Count];
        _currentMarker = SpawnMarker(_currentMarkerEffect, _currentMarkerTarget);
        _nextMarker = SpawnMarker(_nextMarkerEffect, _nextMarkerTarget);

        _checkpoint = Shard.EntityMan.SpawnAreaVisualData(_waypoints[_waypointIndex], new ScopingComponent() { Range = 150 });
        _checkpoint.Encounter = new EncounterComponent()
            {
                EncounterId = entityId,
                Instance = this,
                Events = EncounterComponent.Event.Proximity,
                ProximityDistance = _proximityDistance,
            };
        Shard.EncounterMan.AddCheckingOfProximity(_checkpoint, this);

        _mapMarker = new PersonalMapMarkerData()
            {
                EncounterId = AeroEntityId,
                EncounterMarkerId =
                    new EntityId()
                    {
                        Backing = AeroEntityId.Backing,
                        ControllerId = Controller.Generic,
                    },
                TitleId = data.ChallengeName,
                HasDuration = 0,
                Position = _waypoints[_waypointIndex],
                MarkerType = _mapMarkerRace,
            };
        SoloParticipant.CharacterEntity.AddMapMarker(EntityId, _mapMarker);

        // The client needs the mounted vehicle before it can be held in place, so this waits a moment.
        var raceStartsAt = Shard.CurrentTime + (uint)_countdownMs;
        shard.Post(
            () =>
            {
                if (shard.Encounters.ContainsKey(EntityId))
                {
                    ImmobilizeVehicle(raceStartsAt);
                }
            },
            _immobilizeDelayMs);

        shard.Post(
            () =>
            {
                if (shard.Encounters.ContainsKey(EntityId))
                {
                    StartRace();
                }
            },
            _countdownMs);
    }

    public override RaceView View { get; }

    private bool IsOnFinalWaypoint => View.lap_counterProp == _laps - 1 && _waypointIndex == _waypoints.Count - 1;

    public static uint LeaderboardTypeId(uint zoneId)
    {
        return zoneId switch
               {
                   448  => 2585,
                   1030 => 2593,
                   162  => 2595,
                   _    => 0,
               };
    }

    public void OnExitAttachment(BaseEntity targetEntity, INetworkPlayer player)
    {
        RemoveVehicle(_vehicle);

        OnFailure();
    }

    public void OnProximity(BaseEntity sourceEntity, INetworkPlayer player)
    {
        if (player != SoloParticipant || View.race_startedProp == 0)
        {
            return;
        }

        if (IsOnFinalWaypoint)
        {
            OnSuccess();
            return;
        }

        if (_waypointIndex == _waypoints.Count - 1)
        {
            _waypointIndex = 0;
            View.lap_counterProp++;
            View.waypoint_counterProp = 0;
        }
        else
        {
            _waypointIndex++;
            View.waypoint_counterProp++;
        }

        SetExpireAt(_expireAt + _waypointBonusTimeMs);

        UpdateMarkers();
        Shard.EncounterMan.FlushChanges(this);
    }

    public void OnTimeOut()
    {
        DismountAndRemoveVehicle(_vehicle);

        OnFailure();
    }

    public override void OnSuccess()
    {
        RemoveEntities();
        DismountAndRemoveVehicle(_vehicle);

        var now = Shard.CurrentTimeLong;
        var time = now - _raceStartTime;

        // The challenge is scored by the time left on the objective timer, not by the elapsed time.
        GiveRewards(Math.Max(_expireAt, now) - now);

        _ = GRPCService.SendCommandAsync(new Command()
            {
                SaveLgvRaceFinish = new SaveLgvRaceFinish()
                    {
                        CharacterGuid = SoloParticipant.CharacterId + 0xFE,
                        LeaderboardId = _leaderboardId,
                        TimeMs = time,
                    }
            });

        base.OnSuccess();
    }

    public override void OnFailure()
    {
        RemoveEntities();

        base.OnFailure();
    }

    private void StartRace()
    {
        _raceStartTime = Shard.CurrentTimeLong;

        View.race_startedProp = 1;
        SetExpireAt(_raceStartTime + _objectiveTimeMs);

        Shard.EncounterMan.FlushChanges(this);
    }

    private void SetExpireAt(ulong expireAt)
    {
        _expireAt = expireAt;

        // The timeout check only runs periodically, so the limit may already have passed without having fired yet.
        var remainingMs = Math.Max(_expireAt, Shard.CurrentTimeLong) - Shard.CurrentTimeLong;
        Shard.EncounterMan.SetRemainingLifetime(this, (uint)remainingMs);

        View.objective_timerProp = View.objective_timerProp with { Micro = _expireAt * 1000 };
    }

    private void UpdateMarkers()
    {
        var current = _waypoints[_waypointIndex];

        _checkpoint.SetPosition(current);

        _mapMarker = _mapMarker with { Position = current };
        SoloParticipant.CharacterEntity.UpdateEncounterMapMarker(EntityId, _mapMarker);

        if (IsOnFinalWaypoint)
        {
            RemoveMarkers();
            _finishMarker = SpawnFinishMarker(current);
            return;
        }

        var next = _waypoints[(_waypointIndex + 1) % _waypoints.Count];

        // Starting from the previous target keeps a checkpoint reached mid slide from pulling a marker backwards.
        SlideMarker(_currentMarker, _currentMarkerTarget, current);
        SlideMarker(_nextMarker, _nextMarkerTarget, next);
        _currentMarkerTarget = current;
        _nextMarkerTarget = next;
    }

    private void SlideMarker(DeployableEntity marker, Vector3 from, Vector3 to)
    {
        var now = Shard.CurrentTime;

        marker.Deployable_ObserverView.ForcedMovementDataProp = new ForcedMovementData()
        {
            Type = 3,
            CommandId = 1,
            Params3 = new ForcedMovementType3Params()
            {
                Time1 = now,
                Time2 = now + _markerSlideMs,
                StartPosition = from,
                StartRotation = Quaternion.Identity,
                EndPosition = to,
                EndRotation = Quaternion.Identity,

                // The height follows an arc rather than the end position, so without this it stays level and snaps at the end.
                VerticalVelocity = (to.Z - from.Z) / (_markerSlideMs / 1000f),
                OrientationType = 1,
            },
        };

        // View changes are otherwise sent unreliably, and a lost one would leave the marker behind until the next checkpoint.
        Shard.EntityMan.FlushViewChangesToPlayer(marker.Deployable_ObserverView, marker.EntityId, SoloParticipant);

        Shard.Post(() => marker.SetPosition(to), _markerSlideMs);
    }

    private DeployableEntity SpawnMarker(uint effectId, Vector3 position)
    {
        var marker = Shard.EntityMan.SpawnDeployable(_invisibleDeployable, position, Quaternion.Identity);
        marker.Scoping = new ScopingComponent() { Range = _markerScopeRange };

        // The effect only plays on the client of its initiator, so each racer only sees their own markers.
        Shard.Post(
            () =>
            {
                if (!Shard.Entities.ContainsKey(marker.EntityId))
                {
                    return;
                }

                Shard.Abilities.DoApplyEffect(effectId, marker, new Context(Shard, SoloParticipant.CharacterEntity) { InitTime = Shard.CurrentTime });
                Shard.EntityMan.QueueScopeIn(SoloParticipant, marker);
            },
            _markerDeliveryDelayMs);

        return marker;
    }

    // The finish flag's status effect (8746) has a fixed size, too big for the course, so it's placed as a free
    // particle effect instead. It doesn't slide, so it doesn't need a carrier.
    private AreaVisualDataEntity SpawnFinishMarker(Vector3 position)
    {
        var marker = Shard.EntityMan.SpawnAreaVisualData(position, new ScopingComponent() { Range = _markerScopeRange });
        marker.SetPosition(position);
        marker.AddParticleEffect(new ParticleEffect()
        {
            PfxEntityId = new EntityId() { Backing = GuidService.GetNext(Shard), ControllerId = Controller.Encounter },
            PfxAssetId = _finishLinePfx,
            Position = position + new Vector3(0, 0, _finishLineHeight),
            Rotation = Quaternion.Identity,
            Unk9 = 2,
            StartTime = Shard.CurrentTime,
            Scale = _finishLineScale,
            HaveUnk4 = 0,
            HaveScopeBubbleInfo = 0,
        });

        Shard.Post(() => Shard.EntityMan.QueueScopeIn(SoloParticipant, marker), _markerDeliveryDelayMs);

        return marker;
    }

    private void RemoveMarkers()
    {
        if (_currentMarker != null)
        {
            Shard.EntityMan.Remove(_currentMarker);
            _currentMarker = null;
        }

        if (_nextMarker != null)
        {
            Shard.EntityMan.Remove(_nextMarker);
            _nextMarker = null;
        }

        if (_finishMarker != null)
        {
            Shard.EntityMan.Remove(_finishMarker);
            _finishMarker = null;
        }
    }

    private void GiveRewards(ulong remainingMs)
    {
        var isGold = remainingMs >= _goldTimeMs;
        var bonusVouchers = isGold ? _goldBonusVouchers : 0;
        var vouchers = _rewardVouchers + bonusVouchers;

        Shard.Progression.AwardXp(SoloParticipant, _rewardExperience);
        RewardWithResource(_kisutonVoucher, vouchers);

        var msg = new DisplayRewards()
        {
            IndexId = 1,
            TitleTextId = isGold ? _goldFinishTitle : _silverFinishTitle,
            EventId = 0,
            Stats =
            [
                new StatInfo() { NameId = RewardStatNames.GoldTime, Type = StatInfo.StatType.NumericValue, Value = MathF.Floor(_goldTimeMs / 1000f), StringValue = string.Empty },
                new StatInfo() { NameId = RewardStatNames.Time, Type = StatInfo.StatType.NumericValue, Value = MathF.Floor(remainingMs / 1000f), StringValue = string.Empty },
                new StatInfo() { NameId = RewardStatNames.ExperiencePoints, Type = StatInfo.StatType.NumericValue, Value = _rewardExperience, StringValue = string.Empty },
            ],
            Rewards1 =
            [
                new RewardInfoData() { SdbId = _kisutonVoucher, Quantity = (ushort)vouchers, Boosted = (ushort)bonusVouchers },
            ],
            Rewards2 = [],
            ResourceTargetId = new EntityId() { Backing = 0 },
            ResourceTargetType = 0,
            Experience = 0,
            ScreenType = 0,
            DisplayQuality = 0,
            Reputations = [],
        };

        SoloParticipant.NetChannels[ChannelType.ReliableGss].SendMessage(msg, SoloParticipant.CharacterEntity.EntityId);
    }

    private void ImmobilizeVehicle(uint until)
    {
        var position = _vehicle.Position;

        var message = new VehicleForcedMovement()
        {
            Data = new ForcedMovementData()
            {
                Type = 11,
                CommandId = 1,
                TeleportAndImmobilize = new TeleportAndImmobilize()
                {
                    StartTime = Shard.CurrentTime,
                    EndTime = until,
                    Position = position,
                    Orientation = _vehicle.Orientation,
                },
            },
        };

        SoloParticipant.NetChannels[ChannelType.ReliableGss].SendMessage(message, _vehicle.EntityId);
    }

    private void RemoveEntities()
    {
        _vehicle.Encounter.StopHandling(EncounterComponent.Event.ExitAttachment);
        Shard.EncounterMan.RemoveCheckingOfProximity(_checkpoint);
        Shard.EntityMan.Remove(_checkpoint);
        RemoveMarkers();
        SoloParticipant.CharacterEntity.RemoveEncounterMapMarkers(EntityId);
    }
}
