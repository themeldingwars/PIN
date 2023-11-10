using System;
using Microsoft.AspNetCore.Mvc;

namespace WebHost.InGameApi.Controllers;

[ApiController]
public class SocialDataController : ControllerBase
{
    [Route("social/static_data.json")]
    [Route("api/v1/social/static_data.json")]
    [HttpGet]
    [Produces("application/json")]
    public object StaticData()
    {
        StaticData zones = new StaticData()
        { 
            Zones = new object[]
            {
                new Zone { ZoneId = 12, Title = "Nothing" },
                new Zone { ZoneId = 162, Title = "Diamond Head" },
                
                // new Zone { ZoneId = 204, Title = "REMOVED: Sargasso Sea" },
                new Zone { ZoneId = 448, Title = "New Eden" },
                new Zone { ZoneId = 803, Title = "M15_Mission_15_Agrievan" },
                new Zone { ZoneId = 805, Title = "Epicenter Melding Tornado Pocket" },
                new Zone { ZoneId = 833, Title = "M20EMission_20_Razors_Edge" },
                
                // new Zone { ZoneId = 843, Title = "REMOVED: Antarctica" },
                new Zone { ZoneId = 844, Title = "Omnidyne-M Prototype Stadium" },
                
                // new Zone { ZoneId = 861, Title = "REMOVED: Research Station" },
                // new Zone { ZoneId = 863, Title = "REMOVED: Cliff's Edge" },
                new Zone { ZoneId = 864, Title = "M16EMission_16_Unearthed" },
                new Zone { ZoneId = 865, Title = "Abyss - Melding Tornado Pocket" },
                new Zone { ZoneId = 868, Title = "Cinerarium" },
                
                // new Zone { ZoneId = 878, Title = "REMOVED: Copacabana" },
                new Zone { ZoneId = 1003, Title = "M03EMission_03_Crash_Down" },
                new Zone { ZoneId = 1007, Title = "M18EMission_18_Vagrant_Dawn" },
                new Zone { ZoneId = 1008, Title = "M14EMission_14_Icebreaker" },
                new Zone { ZoneId = 1030, Title = "Sertao" },
                new Zone { ZoneId = 1051, Title = "Baneclaw Lair" },
                new Zone { ZoneId = 1069, Title = "OP01_Operation_Miru" },
                new Zone { ZoneId = 1089, Title = "OP03_Operation_The_ARES_Team" },
                new Zone { ZoneId = 1093, Title = "OP02_Operation_High_Tide" },
                new Zone { ZoneId = 1099, Title = "M09_Mission_09_Taken" },
                new Zone { ZoneId = 1100, Title = "M01_Mission_01_Everything_Is_Shadow" },
                new Zone { ZoneId = 1101, Title = "M07_Mission_07_Trespass" },
                new Zone { ZoneId = 1102, Title = "M04_Mission_04_Razorwind" },
                new Zone { ZoneId = 1104, Title = "M02_Mission_02_Bathsheba" },
                new Zone { ZoneId = 1106, Title = "M10_Mission_10_Off_The_Grid" },
                new Zone { ZoneId = 1113, Title = "M06_Mission_06_Safe_House" },
                new Zone { ZoneId = 1114, Title = "M11_Mission_11_Consequence" },
                new Zone { ZoneId = 1117, Title = "M05_Mission_05_No_Exit" },
                new Zone { ZoneId = 1125, Title = "Battlelab_01" },
                new Zone { ZoneId = 1134, Title = "M08_Mission_08_Catch_Of_The_Day" },
                new Zone { ZoneId = 1147, Title = "Refinery: TDM" },
                new Zone { ZoneId = 1151, Title = "M17_Mission_17_SOS" },
                new Zone { ZoneId = 1154, Title = "M13_Mission_13_Accelerate" },
                new Zone { ZoneId = 1155, Title = "M12EMission_12_Prison_Break" },
                new Zone { ZoneId = 1162, Title = "BattleLab The Danger Room" },
                new Zone { ZoneId = 1163, Title = "Holdout: Jericho" },
                new Zone { ZoneId = 1171, Title = "M19_Mission_19_Gatecrasher" },
                new Zone { ZoneId = 1173, Title = "Raid_01_Defense_of_Dredge" },
                new Zone { ZoneId = 1181, Title = "M22_Mission_22_Homecoming" }
            }
        };
        return zones;
    }
}

public class StaticData
{
    public Array Zones { get; set; }
}

public class Zone
{
    public uint ZoneId { get; set; }
    public string Title { get; set; }
}