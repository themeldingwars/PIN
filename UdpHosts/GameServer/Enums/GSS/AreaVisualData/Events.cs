namespace GameServer.Enums.GSS.AreaVisualData;

// https://github.com/themeldingwars/Documentation/wiki/Messages-AreaVisualData#events
internal enum Events
{
    PartialUpdate = 1, // Delete
    KeyFrame = 4, // Delete
    LootObjectCollected = 83,
    AudioEmitterSpawned = 84,
    ParticleEffectSpawned = 85
}