using AeroMessages.Common;
using GameServer.Entities.Character;
using GameServer.StaticDB.Records.customdata.Encounters;
using GameServer.Systems.Encounters.Encounters;
using LgvRace = GameServer.Systems.Encounters.Encounters.LgvRace;

namespace GameServer.Systems.Encounters;

public class Factory(Shard shard)
{
    public void SpawnEncounter(IEncounterDef def, CharacterEntity initiator)
    {
        var guid = shard.GetNextGuid((byte)Controller.Encounter);

        switch (def)
        {
            case LgvRaceDef lgvRace:
                shard.EncounterMan.Add(guid, new LgvRace(shard, guid, initiator.Player, lgvRace));
                break;
            case KisutonLgvChallengeDef kisutonLgvChallenge:
                shard.EncounterMan.Add(guid, new KisutonLgvChallenge(shard, guid, initiator.Player, kisutonLgvChallenge));
                break;
        }
    }
}