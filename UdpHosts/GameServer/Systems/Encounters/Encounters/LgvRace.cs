using System.Threading;
using AeroMessages.Common;
using AeroMessages.GSS.V66.AreaVisualData;
using AeroMessages.GSS.V66.Character;
using AeroMessages.GSS.V66.Character.Controller;
using AeroMessages.GSS.V66.Character.Event;
using AeroMessages.GSS.V66.Generic.Event.EncounterView;
using GameServer.Entities;
using GameServer.Entities.AreaVisualData;
using GameServer.Entities.Vehicle;
using GameServer.GRPC;
using GameServer.StaticDB.Records.customdata.Encounters;
using GrpcGameServerAPIClient;
using Command = GrpcGameServerAPIClient.Command;
using SinCardTimer = AeroMessages.GSS.V66.Timer;

namespace GameServer.Systems.Encounters.Encounters;

public class LgvRace : BaseEncounter, IExitAttachmentHandler, IProximityHandler, ICanTimeout
{
    private const ushort _terromotoCobra = 48;
    private const uint _crystite = 10;
    private const uint _mustangDoubloon = 77324;
    private const uint _lgvRace = 183671;
    private const uint _finalTime = 152919;
    private const uint _pfxFinishLine = 212597;
    private const ushort _mapMarkerRace = 26;

    public override HudTimerView View { get; }

    private ulong startTime;
    private uint leaderboardId;
    private uint bonusTimeMs;
    private VehicleEntity vehicle;
    private AreaVisualDataEntity finishLine;

    public LgvRace(IShard shard, ulong entityId, INetworkPlayer soloParticipant, LgvRaceDef data)
        : base(shard, entityId, soloParticipant)
    {
        startTime = Shard.CurrentTimeLong;
        Shard.EncounterMan.SetRemainingLifetime(this, data.TimeLimitMs);

        leaderboardId = data.LeaderboardId;

        vehicle = Shard.EntityMan.SpawnVehicle(
            _terromotoCobra,
            data.Start.Position,
            data.Start.Orientation,
            SoloParticipant.CharacterEntity,
            true);
        vehicle.Encounter = new EncounterComponent()
            {
                EncounterId = entityId, Instance = this, Events = EncounterComponent.Event.ExitAttachment
            };

        View = new HudTimerView()
               {
                   hudtimer_labelProp = data.Label,
                   hudtimer_timerProp = new SinCardTimer()
                   {
                       Micro = (startTime + data.TimeLimitMs) * 1000,
                       State = SinCardTimer.TimerState.CountingDown,
                   },
               };

        PlayDialog(data.StartDialog);

        bonusTimeMs = data.BonusTimeLimitMs;
        var bonusTimeDialog = (data.TimeLimitMs - data.BonusTimeLimitMs) switch
            {
                30_000 => 11155u,
                45_000 => 11060u,
                60_000 => 11133u,
                _  => 0u,
            };

        var timer = new Timer(state =>
            {
                if (shard.Encounters.ContainsKey(EntityId))
                {
                    PlayDialog(bonusTimeDialog);
                }

                ((Timer)state)?.Dispose();
            });
        timer.Change(30_000, Timeout.Infinite);

        finishLine = Shard.EntityMan.SpawnAreaVisualData(data.Finish.Position, new ScopingComponent() { Range = 90 });
        finishLine.AreaVisualData_ParticleEffectsView.ParticleEffects_0Prop = new ParticleEffect()
            {
                PfxEntityId = AeroEntityId,
                PfxAssetId = _pfxFinishLine,
                Position = data.Finish.Position,
                Rotation = data.Finish.Orientation,
                Unk9 = 1,
                Unk10 = 1,
                Scale = 0.7f,
                HaveUnk4 = 0,
                HaveUnk12 = 0,
            };
        finishLine.Encounter = new EncounterComponent()
            {
                EncounterId = entityId,
                Instance = this,
                Events = EncounterComponent.Event.Proximity,
                ProximityDistance = 3,
            };
        Shard.EncounterMan.EntitiesToCheckProximity.Add(finishLine, this);

        SoloParticipant.CharacterEntity.AddMapMarker(
            EntityId,
            new PersonalMapMarkerData()
            {
                EncounterId = AeroEntityId,
                EncounterMarkerId =
                    new EntityId()
                    {
                        Backing = AeroEntityId.Backing,
                        ControllerId = Controller.Generic,
                    },
                TitleId = data.Label,
                HasDuration = 0,
                Position = data.Finish.Position,
                MarkerType = _mapMarkerRace,
            });
    }

    public void OnExitAttachment(BaseEntity targetEntity, INetworkPlayer player)
    {
        OnFailure();
    }

    public void OnProximity(BaseEntity sourceEntity, INetworkPlayer player)
    {
        if (player != SoloParticipant)
        {
            return;
        }

        OnSuccess();
    }

    public void OnTimeOut()
    {
        var dialogs = new uint[] { 11015, 11587, 19278 };
        PlayDialog(dialogs[Rng.Next(dialogs.Length)]);

        OnFailure();
    }

    public override void OnSuccess()
    {
        RemoveEntities();

        uint exp;

        var time = Shard.CurrentTimeLong - startTime;

        if (time < bonusTimeMs)
        {
            exp = 5000;
            PlayDialog(11014);
            RewardWithResource(_mustangDoubloon, 1);
        }
        else
        {
            exp = 3500;
            PlayDialog(11586);
        }

        var crystite = (uint)Rng.Next(70, 90);

        RewardWithResource(_crystite, crystite);

        var msg = new DisplayRewards()
                {
                    ResourceTargetId = new EntityId() { Backing = 0 },
                    Experience = exp,
                    Reputations = [],
                    Rewards1 = new RewardInfoData[]
                               {
                                   new RewardInfoData()
                                   {
                                       Unk = 0,
                                       Boosted = 0,
                                       SdbId = _crystite,
                                       Quantity = (ushort)crystite,
                                       Quality = 0,
                                       Module1 = 0,
                                       Module2 = 0,
                                   },
                                   new RewardInfoData()
                                   {
                                       Boosted = 0,
                                       SdbId = _mustangDoubloon,
                                       Quantity = 1,
                                       Quality = 0,
                                       Unk = 0,
                                       Module1 = 0,
                                       Module2 = 0
                                   }
                               },
                    Rewards2 = [],
                    Stats = new StatInfo[]
                            {
                                new StatInfo()
                                {
                                    NameId = _finalTime, Type = 2, Value = time / 1000f, Unk3 = string.Empty,
                                }
                            },
                    Unk1 = 0,
                    IndexId = 1,
                    DisplayQuality = 0,
                    ScreenType = 0,
                    ResourceTargetType = 0,
                    TitleTextId = _lgvRace,
                };

        SoloParticipant.NetChannels[ChannelType.ReliableGss].SendMessage(msg, SoloParticipant.CharacterEntity.EntityId);

        _ = GRPCService.SendCommandAsync(new Command()
            {
                SaveLgvRaceFinish = new SaveLgvRaceFinish()
                    {
                        CharacterGuid = SoloParticipant.CharacterId + 0xFE,
                        LeaderboardId = leaderboardId,
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

    private void RemoveEntities()
    {
        Shard.EntityMan.Remove(finishLine);
        SoloParticipant.CharacterEntity.RemoveEncounterMapMarkers(EntityId);
    }
}