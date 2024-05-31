using System.Collections.Concurrent;
using System.Numerics;
using GameServer.Data;

namespace GameServer.Test;

public static class DataUtils
{
    private static ConcurrentDictionary<uint, Zone> _zones;

    public static void Init()
    {
        _zones = new ConcurrentDictionary<uint, Zone>();

        AddZone(12, "Nothing", 1461290341326u, new Vector3(-9.92f, 0.53f, 0.0f));
        AddZone(162, "Diamond Head", 1461290341098u, new Vector3(-718.07f, 735.62f, 103.0f));
        AddZone(448, "New Eden", 1461290346895u, new Vector3(176.65f, 250.13f, 491.94f), 17);
        _zones[448].POIs.Add("watchtower", new Vector3(176.65f, 250.13f, 491.94f));
        _zones[448].POIs.Add("jacuzzi", new Vector3(-532.0f, -469.0f, 473.0f));
        AddZone(805, "Epicenter Melding Tornado Pocket", 1461290355101u, new Vector3(-112.39f, 60.02f, 536.0f));
        AddZone(844, "Omnidyne-M Prototype Stadium", 1461290362014u, new Vector3(-2.26f, -44.72f, 130.17f));
        AddZone(865, "Abyss - Melding Tornado Pocket", 1461290368902u, new Vector3(0.0f, 0.0f, 0.0f));
        AddZone(868, "Cinerarium", 1461290369176u, new Vector3(54.54f, -129.94f, 356.0f));
        AddZone(1030, "Sertao", 1461290383710u, new Vector3(524.82f, -1110.16f, 773.0f));
        AddZone(1051, "Baneclaw Lair", 1461290384421u, new Vector3(-56.0f, -36.0f, 216.0f));
        AddZone(1069, "OP01_Operation_Miru", 1461290388635u, new Vector3(-253.31f, -238.03f, 60.5f));
        AddZone(1089, "OP03_Operation_The_ARES_Team", 1461290393342u, new Vector3(819.31f, 148.59f, 75.7f));
        AddZone(1093, "OP02_Operation_High_Tide", 1461290397267u, new Vector3(-811.53f, -749.51f, 45.4f));
        AddZone(1100, "M01_Mission_01_Everything_Is_Shadow", 1461290405259u, new Vector3(38.9f, 7.55f, 12.2f));
        AddZone(1104, "M02_Mission_02_Bathsheba", 1461290412668u, new Vector3(-222.88f, -25.55f, 401.4f));
        AddZone(1003, "M03EMission_03_Crash_Down", 1423253380039u, new Vector3(2.49f, -139.21f, 93.4f));
        AddZone(1102, "M04_Mission_04_Razorwind", 1461290409497u, new Vector3(567.44f, -265.96f, 402.2f));
        AddZone(1117, "M05_Mission_05_No_Exit", 1461290428811u, new Vector3(-7.04f, 40.15f, 124.1f));
        AddZone(1113, "M06_Mission_06_Safe_House", 1461290423131u, new Vector3(-55.0f, 14.5f, 124.0f));
        AddZone(1101, "M07_Mission_07_Trespass", 1461290405806u, new Vector3(56.29f, -4.66f, 71.5f));
        AddZone(1134, "M08_Mission_08_Catch_Of_The_Day", 1461290437018u, new Vector3(-107.61f, 224.07f, -10.5f));
        AddZone(1099, "M09_Mission_09_Taken", 1461290398077u, new Vector3(-142.23f, 8.34f, 491.2f));
        AddZone(1106, "M10_Mission_10_Off_The_Grid", 1461290416745u, new Vector3(112.19f, -247.70f, 491.2f));
        AddZone(1114, "M11_Mission_11_Consequence", 1461290424543u, new Vector3(4.8f, 134.89f, 48.3f));
        AddZone(1155, "M12EMission_12_Prison_Break", 1461290446057u, new Vector3(-318.91f, -212.05f, 119.18f));
        AddZone(1154, "M13_Mission_13_Accelerate", 1461290444073u, new Vector3(-85.07f, 72.51f, 13.3f));
        AddZone(1008, "M14EMission_14_Icebreaker", 1461290378632u, new Vector3(104.64f, -99.24f, 23.9f));
        AddZone(803, "M15_Mission_15_Agrievan", 1461290348235u, new Vector3(677.6f, 116.86f, 1008.3f));
        AddZone(864, "M16EMission_16_Unearthed", 1461290364500u, new Vector3(-308.27f, 349.28f, 1.14f));
        AddZone(1151, "M17_Mission_17_SOS", 1461290441302u, new Vector3(67.18f, -29.68f, 77.99f));
        AddZone(1007, "M18EMission_18_Vagrant_Dawn", 1461290375832u, new Vector3(352.81f, 266.75f, 7.9f));
        AddZone(1171, "M19_Mission_19_Gatecrasher", 1461290455548u, new Vector3(-69.75f, -297.67f, 67.9f));
        AddZone(833, "M20EMission_20_Razors_Edge", 1461290358180u, new Vector3(-117.2f, 388.62f, 43.0f));
        AddZone(1181, "M22_Mission_22_Homecoming", 1461290465151u, new Vector3(-325.26f, 54.31f, 13.0f));
        AddZone(1125, "Battlelab_01", 1461290430075u, new Vector3(-63.27f, 4.69f, 217.0f));
        AddZone(1147, "Refinery: TDM", 1461290437107u, new Vector3(-14.57f, -17.03f, -3.0f));
        AddZone(1162, "BattleLab The Danger Room", 1461290451458u, new Vector3(82.26f, -21.89f, 197.0f));
        AddZone(1163, "Holdout: Jericho", 1461290451297u, new Vector3(-10.45f, 260.87f, 386.87f));
        AddZone(1173, "Raid_01_Defense_of_Dredge", 1461290460325u, new Vector3(-116.67f, 67.18f, 191.0f));
    }

    public static Zone GetZone(uint id)
    {
        return _zones.TryGetValue(id, out var zone) ? zone : _zones[448];
    }

    public static string FormatArmyTag(string armyTag)
    {
        return string.IsNullOrEmpty(armyTag) ? string.Empty : "[" + armyTag + "]";
    }

    private static void AddZone(uint id, string name, ulong timestamp, Vector3 spawn, uint defaultOutpostId = 0)
    {
        var zone = new Zone
        {
            ID = id, Name = name, Timestamp = timestamp,
            POIs = { { "origin", new Vector3(0.0f, 0.0f, 0.0f) }, { "spawn", spawn } },
            DefaultOutpostId = defaultOutpostId
        };
        _zones.AddOrUpdate(zone.ID, zone, (_, nc) => nc);
    }
}