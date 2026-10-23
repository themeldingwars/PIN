using GameServer.Entities.Character;
using GameServer.Enums;
using GameServer.StaticDB;

namespace GameServer.Systems.Ai;

/// <summary>
///     Derives NPC movement speeds from SDB data.
///     <c>Monster</c> values win when present; otherwise the <c>Battleframe</c> (chassis) values are used.
///     All speeds are scaled by the live <see cref="AptitudeStat.Speed" /> stat modifier.
/// </summary>
public static class NpcSpeedCalculator
{
    public static NpcSpeeds Calculate(CharacterEntity npc)
    {
        var monster = SDBInterface.GetMonster(npc.StaticInfo.CharacterTypeId);
        var battleframe = SDBInterface.GetBattleframe(npc.CurrentLoadout?.ChassisID ?? 0u);

        float normal = monster?.NormalSpeed ?? 0f;
        float fast = monster?.FastSpeed ?? 0f;
        float crouchMult = battleframe?.CrouchSpeedMult ?? 0f;

        if (normal <= 0f)
        {
            normal = battleframe?.NormalSpeed ?? 0f;
        }

        if (fast <= 0f)
        {
            fast = battleframe?.FastSpeed ?? 0f;
        }

        if (crouchMult <= 0f)
        {
            crouchMult = 0.5f;
        }

        var speedMult = npc.GetCurrentStatModifierValue(AptitudeStat.Speed);

        return new NpcSpeeds(normal * speedMult, fast * speedMult, normal * crouchMult * speedMult);
    }

    /// <summary>
    ///     Body mass for the dynamic physics body. <c>Monster.BodyMass</c> is preferred,
    ///     falling back to <c>Battleframe.BodyMass</c>.
    /// </summary>
    /// <param name="monsterId">The monster id whose <c>BodyMass</c> is preferred.</param>
    /// <param name="chassisId">The battleframe/chassis id whose <c>BodyMass</c> is the fallback.</param>
    /// <returns>The physics body mass for the NPC.</returns>
    public static float BodyMass(uint monsterId, uint chassisId)
    {
        var monster = SDBInterface.GetMonster(monsterId);
        var battleframe = SDBInterface.GetBattleframe(chassisId);

        if (monster is { BodyMass: > 0f })
        {
            return monster.BodyMass;
        }

        if (battleframe is { BodyMass: > 0f })
        {
            return battleframe.BodyMass;
        }

        return 75f;
    }
}
