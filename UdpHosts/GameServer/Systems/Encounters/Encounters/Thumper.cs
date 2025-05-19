using System.Collections.Generic;
using GameServer.Entities;
using GameServer.Entities.Thumper;
using GameServer.Enums;

namespace GameServer.Systems.Encounters.Encounters;

public class Thumper : BaseEncounter, IInteractionHandler
{
    private static readonly uint _updateFrequency = ThumperState.THUMPING.CountdownTime() / 100;
    private ulong _lastUpdate;

    private ThumperEntity thumper;

    public Thumper(IShard shard, ulong entityId, HashSet<INetworkPlayer> participants, ThumperEntity thumperEntity)
        : base(shard, entityId, participants)
    {
        thumper = thumperEntity;

        Shard.EncounterMan.StartUpdatingEncounter(this);
    }

    public void OnInteraction(BaseEntity actingEntity, BaseEntity target)
    {
        switch ((ThumperState)thumper.StateInfo.State)
        {
            case ThumperState.THUMPING:
                Shard.Abilities.HandleActivateAbility(Shard, thumper, thumper.CompletedAbility);

                thumper.TransitionToState(ThumperState.LEAVING);
                break;
            case ThumperState.COMPLETED:
                thumper.StateInfo = thumper.StateInfo with { CountdownTime = Shard.CurrentTime };
                break;
        }
    }

    public override void OnUpdate(ulong currentTime)
    {
        if (Shard.CurrentTime >= thumper.StateInfo.CountdownTime)
        {
            switch ((ThumperState)thumper.StateInfo.State)
            {
                case ThumperState.LANDING:
                    Shard.Abilities.HandleActivateAbility(Shard, thumper, thumper.LandedAbility);
                    break;
                case ThumperState.WARMINGUP:
                    Shard.Abilities.HandleActivateAbility(Shard, thumper, 34579);
                    break;
                case ThumperState.THUMPING:
                    thumper.SetProgress(1);
                    Shard.Abilities.HandleActivateAbility(Shard, thumper, 34215);
                    break;
                case ThumperState.CLOSING:
                    break;
                case ThumperState.COMPLETED:
                    Shard.Abilities.HandleActivateAbility(Shard, thumper, 34216);
                    break;
                case ThumperState.LEAVING:
                    OnSuccess();
                    break;
            }

            if (thumper.StateInfo.State < (byte)ThumperState.LEAVING)
            {
                thumper.TransitionToState((ThumperState)(thumper.StateInfo.State + 1));
            }
        }
        else if (thumper.StateInfo.State == (byte)ThumperState.THUMPING && currentTime > _lastUpdate + _updateFrequency)
        {
            thumper.SetProgress((float)(Shard.CurrentTime - thumper.StateInfo.Time)
                                / (thumper.StateInfo.CountdownTime - thumper.StateInfo.Time));

            _lastUpdate = currentTime;
        }
    }

    public override void OnSuccess()
    {
        Shard.EncounterMan.StopUpdatingEncounter(this);

        Shard.EntityMan.Remove(thumper);

        base.OnSuccess();
    }
}