using System;
using System.Threading.Tasks;

using WebHost.ClientApi.Models;

using Microsoft.AspNetCore.Mvc;

namespace WebHost.ClientApi.Controllers {
	[ApiController]
	public class AccountsController : ControllerBase {
		[Route("api/v2/accounts")]
		[HttpPost]
		public async Task<object> CreateAccount( [FromBody] CreateAccountPost post ) {
			return new { error = false };
		}

		/*[Route("api/v2/accounts")]
		[HttpGet]
		public object AccountsGet() {
			return new { error = false };
		}*/

		[Route("api/v2/accounts/login")]
		[HttpPost]
		public async Task<AccountStatus> Login() {
			return new AccountStatus {
				AccountId = 0x1122334455667788,
				CanLogin = true,
				IsDev = false,
				SteamAuthPrompt = false,
				SkipPrecursor = false,
				CaisStatus = new CaisStatus {
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

		[Route("api/v2/accounts/change_language")]
		[HttpPost]
		public async Task ChangeLanguage() {
			Ok();
		}

		[Route("api/v2/accounts/character_slots")]
		[HttpGet]
		public async Task<object> CharacterSlots() {
			return new { };
		}
	}
}
