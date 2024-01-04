namespace GameServer.Enums.GSS.AreaVisualData;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "https://github.com/themeldingwars/Documentation/wiki/Messages-AreaVisualData#events")]
internal enum Events
{
    PartialUpdate = 1, // Delete
    KeyFrame = 4, // Delete
    LootObjectCollected = 83,
    AudioEmitterSpawned = 84,
    ParticleEffectSpawned = 85
}