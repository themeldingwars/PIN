using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AeroMessages.GSS.V66;
using AeroMessages.GSS.V66.Character.Command;
using AeroMessages.GSS.V66.Character.Event;
using GameServer.Aptitude;
using GameServer.Entities;
using GameServer.Entities.Character;
using GameServer.Entities.Deployable;
using GameServer.Entities.Melding;
using GameServer.StaticDB.Records.customdata.Encounters;

namespace GameServer.Systems.Encounters.Encounters;

public class MeldingRepulsor : BaseEncounter, IInteractionHandler, IDonationHandler, ICanTimeout
{
    private const uint EffectOffline = 2958;
    private const uint EffectOnline = 2957;
    private const uint EffectInitializing = 3285;
    private const uint EffectCollapse = 5157;

    private const uint MaxMeldedCrystite = 4000;
    private const uint PushbackDurationMs = 1224_000;

    private readonly DeployableEntity _repulsor;
    private readonly DeployableEntity _terminal;

    private readonly MeldingEntity _melding;
    private readonly uint _controlPointIndex;
    private Vector3 _startPosition;
    private Vector3 _endPosition;

    private bool IsOnline = false;
    private ulong StartTime = 0;
    private uint MeldedCrystite = 0;

    public MeldingRepulsor(IShard shard, ulong entityId, HashSet<INetworkPlayer> participants, MeldingRepulsorDef repulsorDef)
        : base(shard, entityId, participants)
    {
        var r = repulsorDef.Repulsor;
        _repulsor = Shard.EntityMan.SpawnDeployable(r.Type, r.Position, r.Orientation);
        _repulsor.Encounter = new EncounterComponent { EncounterId = entityId, Instance = this, Events = EncounterComponent.Event.Signal };

        var t = repulsorDef.Terminal;
        _terminal = Shard.EntityMan.SpawnDeployable(t.Type, t.Position, t.Orientation);
        _terminal.Encounter = new EncounterComponent() { EncounterId = entityId, Instance = this, Events = EncounterComponent.Event.Interaction };

        shard.Abilities.DoApplyEffect(EffectOffline, _terminal, new Context(shard, _terminal) { InitTime = shard.CurrentTime });

        _terminal.Deployable_ObserverView.SinCardTypeProp = 109;
        _terminal.Deployable_ObserverView.SinCardFields_11Prop = new SinCardFieldData()
            {
                Type = SinCardFieldData.SincardFieldDataType.Float,
                Float = MaxMeldedCrystite,
            };

        _melding = (MeldingEntity)Shard.Entities.Values.First(e => e is MeldingEntity meldingEntity
            && meldingEntity.PerimiterSetName == repulsorDef.PerimiterSetName);

        var adp = _melding.Melding_ObserverView.ActiveDataProp;

        _controlPointIndex = repulsorDef.MeldingPosition.ControlPointIndex;

        _startPosition = repulsorDef.MeldingPosition.Position;
        _endPosition = adp.ControlPoints_1[_controlPointIndex];

        adp.ControlPoints_1[_controlPointIndex] = _startPosition;
        adp.ControlPoints_2[_controlPointIndex] = _startPosition;
        _melding.SetActiveData(adp);
    }

    public void OnInteraction(BaseEntity actingEntity, BaseEntity target)
    {
        if (actingEntity is not CharacterEntity { IsPlayerControlled: true } character
            || target != _terminal)
        {
            return;
        }

        var uiQuery = new NewUiQuery()
                      {
                          QueryGuid = Shard.GetNextGuid((byte)Enums.GSS.Controllers.Generic),
                          Type = 3,
                          Prompt = 184507,
                          Inputs = new NewUiQueryInput[]
                                   {
                                       new NewUiQueryInput()
                                       {
                                           Id = 4,
                                           Value = MaxMeldedCrystite - MeldedCrystite,
                                       }
                                   }
                      };

        Shard.EncounterMan.SendUiQuery(uiQuery, character.Player, this);
    }

    public void OnDonation(UiQueryResponse response, INetworkPlayer player)
    {
        Participants.Add(player);

        MeldedCrystite += (uint)response.Outputs[0].Amount;

        _terminal.Deployable_ObserverView.SinCardFields_11Prop = new SinCardFieldData()
            {
                Type = SinCardFieldData.SincardFieldDataType.Float,
                Float = MaxMeldedCrystite - MeldedCrystite,
            };

        if (MeldedCrystite < MaxMeldedCrystite)
        {
            return;
        }

        _terminal.Encounter.StopHandling(EncounterComponent.Event.Interaction);

        Shard.Abilities.DoRemoveEffect(_terminal, EffectOffline);
        Shard.Abilities.DoApplyEffect(EffectInitializing, _terminal, new Context(Shard, _terminal) { InitTime = Shard.CurrentTime });

        Shard.Abilities.HandleActivateAbility(Shard, _repulsor, _repulsor.Interaction.CompletedAbilityId);
    }

    public override void OnSignal()
    {
        StartTime = Shard.CurrentTimeLong;

        Shard.EncounterMan.StartUpdatingEncounter(this);

        Shard.Abilities.DoRemoveEffect(_terminal, EffectInitializing);
        Shard.Abilities.DoApplyEffect(EffectOnline, _terminal, new Context(Shard, _terminal) { InitTime = Shard.CurrentTime });

        Shard.EncounterMan.SetRemainingLifetime(this, PushbackDurationMs);
        _terminal.Deployable_ObserverView.SinCardFields_3Prop = new SinCardFieldData()
            {
                Type = SinCardFieldData.SincardFieldDataType.Timer,
                Timer = new Timer()
                        {
                            State = Timer.TimerState.CountingDown,
                            Micro = (Shard.CurrentTimeLong + PushbackDurationMs) * 1000,
                        },
            };
    }

    public void OnTimeOut()
    {
        foreach (var participant in Participants)
        {
            Shard.Abilities.DoApplyEffect(EffectCollapse, participant.CharacterEntity, new Context(Shard, _repulsor) { InitTime = Shard.CurrentTime });
        }

        Shard.Abilities.DoApplyEffect(9487, _repulsor, new Context(Shard, _repulsor) { InitTime = Shard.CurrentTime });

        Shard.Abilities.DoRemoveEffect(_terminal, EffectOnline);
        Shard.Abilities.DoApplyEffect(EffectOffline, _terminal, new Context(Shard, _terminal) { InitTime = Shard.CurrentTime });

        StartTime = Shard.CurrentTimeLong;

        Shard.EncounterMan.StartUpdatingEncounter(this);

        MeldedCrystite = 0;
        _terminal.Deployable_ObserverView.SinCardFields_3Prop = null;
        _terminal.Deployable_ObserverView.SinCardFields_11Prop = new SinCardFieldData()
            {
                Type = SinCardFieldData.SincardFieldDataType.Float,
                Float = MaxMeldedCrystite - MeldedCrystite,
            };
    }

    public override void OnUpdate(ulong currentTime)
    {
        float elapsedTime = Math.Min(currentTime - StartTime, 22_000);
        float progress = (float)(-(Math.Cos(Math.PI * (elapsedTime / 22_000)) - 1) / 2);
        Vector3 newPosition = Vector3.Lerp(_startPosition, _endPosition, progress);

        var activeData = _melding.Melding_ObserverView.ActiveDataProp;

        activeData.ControlPoints_1[_controlPointIndex] = newPosition;
        activeData.ControlPoints_2[_controlPointIndex] = newPosition;

        _melding.SetActiveData(activeData);

        if (newPosition != _endPosition)
        {
            return;
        }

        Shard.EncounterMan.StopUpdatingEncounter(this);

        IsOnline = !IsOnline;

        (_startPosition, _endPosition) = (_endPosition, _startPosition);

        if (!IsOnline)
        {
            _terminal.Encounter.StartHandling(EncounterComponent.Event.Interaction);
        }
    }
}