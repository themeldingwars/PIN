namespace GameServer.Data.SDB.Records.vcs;

// There are 30 distinct sdb_guid values in vcs::BaseComponentDef
// Each sdb_guid appears in only one of <Type>ComponentDef tables
public enum ComponentType : uint
{
    Scoping = 3866376917,
    Driver = 1562371278,
    Passenger = 2202208390,
    Ability = 3048110324,
    Damage = 3737859927,
    StatusEffect = 3322729247,
    Turret = 533726263,
    Deployable = 3213454533,
    SpawnPoint = 1715213540,

    HullSegment = 555024461,
    Warpaint = 1238758490,
    WheelBase = 2550758801,
    Transmission = 3101116909,
    Suspension = 2505221195,
    Engine  = 868401361,
    Steering = 4196128104,
    Braking = 3431260985,
    Aerodynamics = 1921341588,
    HeadLight = 2674988324,
    Exhaust = 2154345371,
    FlightPath = 2075941479,
    Visual = 2131841188,
    Light = 1648495344,
    LightSensor = 1780124415,
    GroundEffects = 3716493569,
    Camera = 4175714783,
    GroundPath = 1292643894,
    SimpleShieldGenerator = 22217899,
    DropPod = 1851570801,
    Cloak = 3678315516,
}