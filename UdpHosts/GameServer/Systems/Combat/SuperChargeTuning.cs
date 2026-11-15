using GameServer.StaticDB;

namespace GameServer.Systems.Combat;

public sealed record SuperChargeTuning(
    float PercentPerDamageDealt,
    float PercentPerHealDealt,
    float PercentPerDamageTaken,
    float IdleGainPercent,
    float IdleGainDurationMs,
    float IdleHoldDurationMs,
    float IdleMaxDecayRate,
    float IdleMaxDecayDurationMs,
    float CombatTimeoutMs)
{
    public static SuperChargeTuning FromStaticDb()
    {
        return new SuperChargeTuning(
            SDBInterface.GetConsoleVariableFloat("superCharge.percentPerDamageDealt", 0.00001f),
            SDBInterface.GetConsoleVariableFloat("superCharge.percentPerHealDealt", 0.0001f),
            SDBInterface.GetConsoleVariableFloat("superCharge.percentPerDamageTaken", 0.0005f),
            SDBInterface.GetConsoleVariableFloat("superCharge.idleGainPercent", 4.1666f),
            SDBInterface.GetConsoleVariableFloat("superCharge.idleGainDuration", 10000f),
            SDBInterface.GetConsoleVariableFloat("superCharge.idleHoldDuration", 60000f),
            SDBInterface.GetConsoleVariableFloat("superCharge.idleMaxDecayRate", 0.2f),
            SDBInterface.GetConsoleVariableFloat("superCharge.idleMaxDecayDuration", 60000f),
            SDBInterface.GetConsoleVariableFloat("combat.timeout", 10000f));
    }
}