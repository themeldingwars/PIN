using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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

    public class CharatersList
    {
        public IEnumerable<Character> Characters { get; set; }
        public bool IsDev { get; set; }
        public int RbBalance { get; set; }
        public int NameChangeCost { get; set; }
    }

    public class Character
    {
        public long CharacterGuid { get; set; }
        public string Name { get; set; }
        public string UniqueName { get; set; }
        public bool IsDev { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TitleId { get; set; }
        public int TimePlayedSecs { get; set; }
        public bool NeedsNameChange { get; set; }
        public int MaxFrameLevel { get; set; }
        public int FrameSdbId { get; set; }
        public int CurrentLevel { get; set; }
        public int Gender { get; set; }
        public string CurrentGender { get; set; }
        public int EliteRank { get; set; }
        public DateTime LastSeenAt { get; set; }
        public Visuals Visuals { get; set; }
        public IEnumerable<Gear> Gear { get; set; }
        public int ExpiresIn { get; set; }
        public string Race { get; set; }
        public IEnumerable<int> Migrations { get; set; }
    }

    public class Gear
    {
        public int SlotTypeId { get; set; }
        public int SdbId { get; set; }
        public long ItemGuid { get; set; }
    }

    public class Visuals
    {
        public int Id { get; set; }
        public int Race { get; set; }
        public int Gender { get; set; }
        public ColoredItem SkinColor { get; set; }
        public Item VoiceSet { get; set; }
        public Item Head { get; set; }
        public ColoredItem EyeColor { get; set; }
        public ColoredItem LipColor { get; set; }
        public ColoredItem HairColor { get; set; }
        public ColoredItem FacialHairColor { get; set; }
        public IEnumerable<ColoredItem> HeadAccessories { get; set; }
        public IEnumerable<ColoredItem> Ornaments { get; set; }
        public Item Eyes { get; set; }
        public HairItem Hair { get; set; }
        public HairItem FacialHair { get; set; }
        public Item Glider { get; set; }
        public Item Vehicle { get; set; }
        public IEnumerable<ColoredTransformableSdbItem> Decals { get; set; }
        public int WarpaintId { get; set; }
        public IEnumerable<long> Warpaint { get; set; }
        public IEnumerable<long> Decalgradients { get; set; }
        public IEnumerable<WarpaintPattern> WarpaintPatterns { get; set; }
        public IEnumerable<long> VisualOverrides { get; set; }
    }

    public class Item
    {
        public int Id { get; set; }
    }

    public class SdbItem
    {
        public int SdbId { get; set; }
    }

    public class TransformableSdbItem : SdbItem
    {
        public IEnumerable<decimal> Transform { get; set; }
    }

    public class ColoredTransformableSdbItem : TransformableSdbItem
    {
        public long Color { get; set; }
    }

    public class WarpaintPattern : TransformableSdbItem
    {
        public int Usage { get; set; }
    }

    public class ColoredItem : Item
    {
        public ColorValue Value { get; set; }
    }

    public class HairItem : Item
    {
        public ColorItem Color { get; set; }
    }

    public class ColorItem : Item
    {
        public long Value { get; set; }
    }

    public class ColorValue
    {
        public long Color { get; set; }
    }

    public class AccountStatus
    {
        public long AccountId { get; set; }
        public bool CanLogin { get; set; }
        public bool IsDev { get; set; }
        public bool SteamAuthPrompt { get; set; }
        public bool SkipPrecursor { get; set; }
        public CaisStatus CaisStatus { get; set; }
        public int CharacterLimit { get; set; }
        public bool IsVip { get; set; }
        public long VipExpiration { get; set; }
        public long CreatedAt { get; set; }
    }

    public class CaisStatus
    {
        public string State { get; set; }
        public long Duration { get; set; }
        public long ExpiresAt { get; set; }
    }
}