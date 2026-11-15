using GameServer.StaticDB;

namespace GameServer.Entities.Character;

public sealed record JetEnergyTuning(float EnergyUsage, float EnergyUsageSprinting, float SprintDashEnergy, float MaxClimbSpeed, float TooFastEnergyMult)
{
    public static JetEnergyTuning FromStaticDb()
    {
        return new JetEnergyTuning(
            SDBInterface.GetConsoleVariableFloat("jetpack.EnergyUsage", 100f),
            SDBInterface.GetConsoleVariableFloat("jetpack.EnergyUsageSprinting", 150f),
            SDBInterface.GetConsoleVariableFloat("jetpack.SprintDashEnergy", 60f),
            SDBInterface.GetConsoleVariableFloat("jetpack.MaxClimbSpeed", 5f),
            SDBInterface.GetConsoleVariableFloat("jetpack.TooFastEnergyMult", 0.5f));
    }
}