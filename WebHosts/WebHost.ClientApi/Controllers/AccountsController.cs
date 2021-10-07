using Microsoft.AspNetCore.Mvc;
using System;
using WebHost.ClientApi.Models.Accounts;

namespace WebHost.ClientApi.Controllers
{
    [ApiController]
    public class AccountsController : ControllerBase
    {
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
                       CharacterLimit = 2,
                       IsVip = false,
                       VipExpiration = 0,
                       CreatedAt = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()
                   };
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
            return new { };
        }
    }
}