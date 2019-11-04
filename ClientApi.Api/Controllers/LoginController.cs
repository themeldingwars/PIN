using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using ClientApi.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ClientApi.Api.Controllers
{
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;

        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
        }

        [Route("api/v1/login_alerts")]
        [HttpGet]
        public HttpResponseMessage GetLoginAlerts()
        {
            var logInfo = $"Endpoint: {HttpContext.Request.Host}{HttpContext.Request.Path}\nHeaders:\n";
            logInfo =
                HttpContext.Request.Headers.Aggregate(logInfo,
                                                      (current, header) =>
                                                          current + $"\t{header.Key}: {header.Value}\n");
            _logger.LogDebug(logInfo);

            const string Html = @"<html>
<head>
<meta charset=""utf - 8"" />
<title>InGame</title>
</head>
<body>
<strong>Welcome to SIN</strong><br>
The Shared Intelligence Network is here to serve you all the content you'd want
</body>
</html>";

            return new HttpResponseMessage
                   {
                       Content = new StringContent(Html,
                                                   Encoding.UTF8,
                                                   "text/html")
                   };
        }

        [Route("api/v2/accounts/login")]
        [HttpPost]
        public AccountStatus AccountsLogin()
        {
            return new AccountStatus
                   {
                       AccountId = 1337,
                       CanLogin = true,
                       IsDev = false,
                       SteamAuthPrompt = false,
                       SkipPrecursor = false,
                       CaisStatus = new CaisStatus
                                    {
                                        Duration = 0,
                                        ExpiresAt = 0,
                                        State = "disabled"
                                    },
                       CharacterLimit = 2,
                       IsVip = false,
                       VipExpiration = 0,
                       CreatedAt = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()
                   };
        }

        [Route("/api/v1/oracle/ticket")]
        [HttpPost]
        public void GetOracleTicket()
        {

        }
        
        [Route("api/v2/accounts/change_language")]
        [HttpPost]
        public void ChangeLanguage()
        {
            Ok();
        }

        [Route("api/v1/server/list")]
        [HttpPost]
        public ServerList GetServerList()
        {
            return new ServerList();
        }

        [Route("api/v2/characters/list")]
        [HttpGet]
        public CharatersList GetCharactersList()
        {
            return new CharatersList
                   {
                       Characters = new List<Character>
                                    {
                                        new Character
                                        {
                                            CharacterGuid = 42,
                                            Name = "Ascendant",
                                            UniqueName = "Ascendant",
                                            IsDev = false,
                                            IsActive = true,
                                            CreatedAt = new DateTime(2017, 1, 3, 23, 41, 26),
                                            TitleId = 0,
                                            TimePlayedSecs = 500,
                                            NeedsNameChange = false,
                                            MaxFrameLevel = 10,
                                            FrameSdbId = 76334,
                                            CurrentLevel = 10,
                                            Gender = 1,
                                            CurrentGender = "female",
                                            EliteRank = 95487,
                                            LastSeenAt = DateTime.Now - TimeSpan.FromDays(365),
                                            Visuals = new Visuals
                                                      {
                                                          Id = 0,
                                                          Race = 0,
                                                          Gender = 1,
                                                          SkinColor = new ColoredItem
                                                                      {
                                                                          Id = 118969,
                                                                          Value = new ColorValue
                                                                                  {
                                                                                      Color = 4294930822
                                                                                  }
                                                                      },
                                                          VoiceSet = new Item
                                                                     {
                                                                         Id = 1033
                                                                     },
                                                          Head = new Item
                                                                 {
                                                                     Id = 10026
                                                                 },
                                                          EyeColor = new ColoredItem
                                                                     {
                                                                         Id = 118980,
                                                                         Value = new ColorValue
                                                                                 {
                                                                                     Color = 1633685600
                                                                                 }
                                                                     },
                                                          LipColor = new ColoredItem
                                                                     {
                                                                         Id = 1,
                                                                         Value = new ColorValue
                                                                                 {
                                                                                     Color = 4294903873
                                                                                 }
                                                                     },
                                                          HairColor = new ColoredItem
                                                                      {
                                                                          Id = 77193,
                                                                          Value = new ColorValue
                                                                                  {
                                                                                      Color = 1917780001
                                                                                  }
                                                                      },
                                                          FacialHairColor = new ColoredItem
                                                                            {
                                                                                Id = 77193,
                                                                                Value = new ColorValue
                                                                                        {
                                                                                            Color = 1917780001
                                                                                        }
                                                                            },
                                                          HeadAccessories = new List<ColoredItem>
                                                                            {
                                                                                new ColoredItem
                                                                                {
                                                                                    Id = 10117,
                                                                                    Value = new ColorValue
                                                                                            {
                                                                                                Color = 1211031763
                                                                                            }
                                                                                }
                                                                            },
                                                          Ornaments = new List<ColoredItem>(),
                                                          Eyes = new Item
                                                                 {
                                                                     Id = 10001
                                                                 },
                                                          Hair = new HairItem
                                                                 {
                                                                     Id = 10113,
                                                                     Color = new ColorItem
                                                                             {
                                                                                 Id = 77193,
                                                                                 Value = 1917780001
                                                                             }
                                                                 },
                                                          FacialHair = new HairItem
                                                                       {
                                                                           Id = 0,
                                                                           Color = new ColorItem
                                                                                   {
                                                                                       Id = 77187,
                                                                                       Value = 1518862368
                                                                                   }
                                                                       },
                                                          Glider = new Item
                                                                   {
                                                                       Id = 0
                                                                   },
                                                          Vehicle = new Item
                                                                    {
                                                                        Id = 0
                                                                    },
                                                          Decals = new List<ColoredTransformableSdbItem>(),
                                                          WarpaintId = 143225,
                                                          Warpaint = new List<long>
                                                                     {
                                                                         4216738474,
                                                                         0,
                                                                         4216717312,
                                                                         418250752,
                                                                         1525350400,
                                                                         4162844703,
                                                                         4162844703
                                                                     },
                                                          Decalgradients = new List<long>(),
                                                          WarpaintPatterns = new List<WarpaintPattern>(),
                                                          VisualOverrides = new List<long>()
                                                      },
                                            Gear = new List<Gear>
                                                   {
                                                       new Gear
                                                       {
                                                           SlotTypeId = 1,
                                                           SdbId = 86969,
                                                           ItemGuid = 5068916056568384765
                                                       },
                                                       new Gear
                                                       {
                                                           SlotTypeId = 2,
                                                           SdbId = 87918,
                                                           ItemGuid = 5068916056568385021
                                                       },
                                                       new Gear
                                                       {
                                                           SlotTypeId = 6,
                                                           SdbId = 91770,
                                                           ItemGuid = 5068923373180718589
                                                       },
                                                       new Gear
                                                       {
                                                           SlotTypeId = 116,
                                                           SdbId = 126000,
                                                           ItemGuid = 5068916056568385277
                                                       },
                                                       new Gear
                                                       {
                                                           SlotTypeId = 122,
                                                           SdbId = 129359,
                                                           ItemGuid = 5068916056568385533
                                                       },
                                                       new Gear
                                                       {
                                                           SlotTypeId = 126,
                                                           SdbId = 127501,
                                                           ItemGuid = 5068916056568385789
                                                       },
                                                       new Gear
                                                       {
                                                           SlotTypeId = 127,
                                                           SdbId = 128271,
                                                           ItemGuid = 5068916056568386045
                                                       },
                                                       new Gear
                                                       {
                                                           SlotTypeId = 128,
                                                           SdbId = 126731,
                                                           ItemGuid = 5068916056568386301
                                                       },
                                                       new Gear
                                                       {
                                                           SlotTypeId = 129,
                                                           SdbId = 129067,
                                                           ItemGuid = 5068916056568386557
                                                       }
                                                   },
                                            ExpiresIn = 0,
                                            Race = "chosen",
                                            Migrations = new List<int>()
                                        }
                                    },
                       IsDev = false,
                       RbBalance = 0,
                       NameChangeCost = 100
                   };
        }
    }
}