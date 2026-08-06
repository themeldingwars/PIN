using GameServer.Entities.Character;
using GameServer.Systems.CharacterLifecycle;
using GameServer.Systems.SystemEvents;
using Serilog;

namespace GameServer.Systems.NpcDeath;

public class NpcDeathService
{
    private static readonly ILogger Logger = Log.ForContext<NpcDeathService>();

    private readonly IShard _shard;
    private readonly INpcDeathRules _rules;

    public NpcDeathService(IShard shard, IEventBus eventBus, INpcDeathRules rules)
    {
        _shard = shard;
        _rules = rules;

        eventBus.Subscribe<CharacterDiedEvent>(OnCharacterDied);
    }

    private void OnCharacterDied(CharacterDiedEvent evt)
    {
        var character = evt.Target;
        if (character.IsPlayerControlled)
        {
            return;
        }

        if (character.TryGetGibVisualsId(out var gibVisualsId))
        {
            character.SetGibVisualsInfo(gibVisualsId, character.Shard.CurrentTime);
        }
        else
        {
            Logger.Debug("No gib visuals id available for {Name}, skipping gib visuals", character);
        }

        _shard.EntityMan.SetRemainingLifetime(character, (uint)_rules.CorpseLingerMs);
    }
}
