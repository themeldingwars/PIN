using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WebHost.ClientApi.Characters.Models;
using WebHost.ClientApi.Models.Accounts;

namespace WebHost.ClientApi.Controllers;

[ApiController]
public class AccountsController : ControllerBase
{
    private static ConcurrentDictionary<uint, GarageSlots> _garageSlots;

    [Route("api/v2/accounts")]
    [HttpPost]
    public object CreateAccount([FromBody] CreateAccountPost post)
    {
        return new { error = false };
    }

    [Route("api/v2/accounts/login")]
    [HttpPost]
    public AccountStatus Login()
    {
        return new AccountStatus
               {
                   AccountId = 0x1122334455667788,
                   CanLogin = true,
                   IsDev = false,
                   SteamAuthPrompt = false,
                   SkipPrecursor = false,
                   CaisStatus = new CaisStatus { Duration = 0, ExpiresAt = 0, State = "disabled" },
                   CharacterLimit = 40,
                   IsVip = true,
                   VipExpiration = 0,
                   CreatedAt = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()
               };
    }

    [Route("api/v2/accounts/current/status")]
    [HttpGet]
    public object CurrentStatus()
    {
        return new CurrentStatus { IsActive = true, CanLogin = true, IsDev = false, IsBanned = false };
    }

    [Route("api/v2/accounts/change_language")]
    [HttpPost]
    public void ChangeLanguage()
    {
        Ok();
    }

    [Route("api/v2/accounts/character_slots")]
    [HttpGet]
    public object CharacterSlots()
    {
        return new List<Gear>
               {
                   new() { SlotTypeId = 1, SdbId = 86969, ItemGuid = 5068916056568384765 },
                   new() { SlotTypeId = 2, SdbId = 87918, ItemGuid = 5068916056568385021 },
                   new() { SlotTypeId = 6, SdbId = 91770, ItemGuid = 5068923373180718589 },
                   new() { SlotTypeId = 116, SdbId = 126000, ItemGuid = 5068916056568385277 },
                   new() { SlotTypeId = 122, SdbId = 129359, ItemGuid = 5068916056568385533 },
                   new() { SlotTypeId = 126, SdbId = 127501, ItemGuid = 5068916056568385789 },
                   new() { SlotTypeId = 127, SdbId = 128271, ItemGuid = 5068916056568386045 },
                   new() { SlotTypeId = 128, SdbId = 126731, ItemGuid = 5068916056568386301 },
                   new() { SlotTypeId = 129, SdbId = 129067, ItemGuid = 5068916056568386557 }
               };
    }

    [Route("api/v3/characters/{characterId}/garage_slots")]
    [HttpGet]
    [Produces("application/json")]
    public object GarageSlots(string characterId)
    {
        _garageSlots = new ConcurrentDictionary<uint, GarageSlots>();

        var CraftingStation = new GarageSlots
                              {
                                  Id = 123987212,
                                  Name = "Crafting Station",
                                  CharacterGuid = ulong.Parse(characterId),
                                  GarageType = "crafting_station",
                                  ItemGuid = 9161555162510396669,
                                  EquippedSlots = Array.Empty<Array>(),
                                  Limits = new ItemLimits { Abilities = 4 },
                                  Decals = Array.Empty<Array>(),
                                  VisualLoadoutId = 0,
                                  WarpaintId = 0,
                                  Warpaintpatterns = Array.Empty<Array>(),
                                  VisualOverrides = Array.Empty<Array>(),
                                  Unlocked = true,
                                  ExpiresInSecs = 0
                              };
        _garageSlots.AddOrUpdate(CraftingStation.Id, CraftingStation, (k, nc) => nc);

        var Firecat = new GarageSlots
                      {
                          Id = 184534131,
                          Name = "Astrek \"Firecat\"",
                          CharacterGuid = ulong.Parse(characterId),
                          GarageType = "battleframe",
                          ItemGuid = 9215052991608503805,
                          EquippedSlots = Array.Empty<Array>(),
                          Limits = new ItemLimits { Abilities = 4 },
                          Decals = new object[]
                                   {
                                       new Decals
                                       {
                                           SdbId = 10000,
                                           Color = 4294967295,
                                           Transform = new object[]
                                                       {
                                                           0.05246, 0.019623, 0.0, 0.007484, -0.02002, -0.051758,
                                                           0.018127, -0.048492, 0.021362, 0.108154, -0.105469, 1.495117
                                                       }
                                       }
                                   },
                          VisualLoadoutId = 31240112,
                          WarpaintId = 77307,
                          Warpaintpatterns =
                              new object[]
                              {
                                  new Warpaintpatterns
                                  {
                                      SdbId = 10022, Transform = new object[] { 0.0, 16384.0, 418.0, 0.0 }, Usage = 0
                                  }
                              },
                          VisualOverrides = Array.Empty<Array>(),
                          Unlocked = true,
                          ExpiresInSecs = 0
                      };
        _garageSlots.AddOrUpdate(Firecat.Id, Firecat, (k, nc) => nc);

        return _garageSlots.Values;
    }

    // Temporary location
    [Route("api/v3/ui_actions")]
    [HttpPost]
    public void UIActions()
    {
        /*
         * POST is the following (example):
         *[
         *  {
         *    "screen_reference_id":48808253,
         *    "screen":"crafting",
         *    "action":"open"
         *  }
         *]
         *
         * What is this for? Logging?
         * Return seems to always have been empty.
         */

        Ok();
    }
}

public class CurrentStatus
{
    public bool IsActive { get; set; }
    public bool CanLogin { get; set; }
    public bool IsDev { get; set; }
    public bool IsBanned { get; set; }
}