namespace GameServer.Systems.NpcDeath;

public class StandardNpcDeathRules : INpcDeathRules
{
    public int CorpseLingerMs { get; init; } = 10_000;

    public int DestroyedDeployableLingerMs { get; init; } = 1_000;
}
