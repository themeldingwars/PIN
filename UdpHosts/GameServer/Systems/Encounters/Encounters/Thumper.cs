using System.Collections.Generic;
using GameServer.Entities;
using GameServer.Entities.Thumper;
using GameServer.Enums;

namespace GameServer.Systems.Encounters.Encounters;

public class Thumper : BaseEncounter, IInteractionHandler
{
    private static readonly uint _updateFrequency = ThumperState.THUMPING.CountdownTime() / 100;
    private readonly ThumperEntity _thumper;
    private ulong _lastUpdate;

    public Thumper(IShard shard, ulong entityId, HashSet<INetworkPlayer> participants, ThumperEntity thumperEntity)
        : base(shard, entityId, participants)
    {
        _thumper = thumperEntity;

        Shard.EncounterMan.StartUpdatingEncounter(this);
    }

    public void OnInteraction(BaseEntity actingEntity, BaseEntity target)
    {
        switch ((ThumperState)_thumper.StateInfo.State)
        {
            case ThumperState.THUMPING:
                Shard.Abilities.HandleActivateAbility(Shard, _thumper, _thumper.CompletedAbility);

                _thumper.TransitionToState(ThumperState.LEAVING);
                break;
            case ThumperState.COMPLETED:
                _thumper.StateInfo = _thumper.StateInfo with { CountdownTime = Shard.CurrentTime };
                break;
        }
    }

    public override void OnUpdate(ulong currentTime)
    {
        if (Shard.CurrentTime >= _thumper.StateInfo.CountdownTime)
        {
            switch ((ThumperState)_thumper.StateInfo.State)
            {
                case ThumperState.LANDING:
                    Shard.Abilities.HandleActivateAbility(Shard, _thumper, _thumper.LandedAbility);
                    break;
                case ThumperState.WARMINGUP:
                    Shard.Abilities.HandleActivateAbility(Shard, _thumper, 34579);
                    break;
                case ThumperState.THUMPING:
                    _thumper.SetProgress(1);
                    Shard.Abilities.HandleActivateAbility(Shard, _thumper, 34215);
                    break;
                case ThumperState.CLOSING:
                    break;
                case ThumperState.COMPLETED:
                    Shard.Abilities.HandleActivateAbility(Shard, _thumper, 34216);
                    break;
                case ThumperState.LEAVING:
                    OnSuccess();
                    break;
            }

            if (_thumper.StateInfo.State < (byte)ThumperState.LEAVING)
            {
                _thumper.TransitionToState((ThumperState)(_thumper.StateInfo.State + 1));
            }
        }
        else if (_thumper.StateInfo.State == (byte)ThumperState.THUMPING && currentTime > _lastUpdate + _updateFrequency)
        {
            _thumper.SetProgress((float)(Shard.CurrentTime - _thumper.StateInfo.Time)
                                / (_thumper.StateInfo.CountdownTime - _thumper.StateInfo.Time));

            _lastUpdate = currentTime;
        }
    }

    public override void OnSuccess()
    {
        Shard.EncounterMan.StopUpdatingEncounter(this);

        Shard.EntityMan.Remove(_thumper);

        base.OnSuccess();
    }
}