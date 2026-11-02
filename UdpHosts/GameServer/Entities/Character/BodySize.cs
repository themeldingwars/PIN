using System.Collections.Concurrent;
using GameServer.StaticDB;

namespace GameServer.Entities.Character;

/// <summary>
///     The size a character moves with in meters: the capsule of its battleframe's pose type, scaled like the
///     battleframe. These are Red 5's own numbers, and unlike the physics body they're known from the monster id before
///     anything spawns.
/// </summary>
public readonly record struct BodySize(float Radius, float Height)
{
    /// <summary>For a chassis whose battleframe or pose type is missing from the SDB.</summary>
    public static readonly BodySize Biped = new(0.5f, 1.8f);

    private static readonly ConcurrentDictionary<uint, BodySize> _byChassis = new();

    public static BodySize ForMonster(uint monsterId) => SDBInterface.GetMonster(monsterId) is { } monster ? ForChassis(monster.ChassisId) : Biped;

    public static BodySize ForChassis(uint chassisId) => _byChassis.GetOrAdd(chassisId, Read);

    private static BodySize Read(uint chassisId)
    {
        if (SDBInterface.GetBattleframe(chassisId) is not { } battleframe || SDBInterface.GetPoseType(battleframe.PosetypeId) is not { PhysicsRadius: > 0f } pose)
        {
            return Biped;
        }

        var scale = battleframe.MinRandScale > 0f ? battleframe.MinRandScale : 1f;
        return new BodySize(pose.PhysicsRadius * scale, pose.PhysicsHeight * scale);
    }
}
