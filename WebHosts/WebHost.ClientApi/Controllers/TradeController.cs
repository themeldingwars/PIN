using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebHost.ClientApi.Controllers {
	[ApiController]
	public class TradeController : ControllerBase {
		[Route("api/v3/trade/products/garage_slot_perk_respec")]
		[HttpGet]
		public object GarageSlotPerkRespec() {
			return new { };
		}
	}
}
