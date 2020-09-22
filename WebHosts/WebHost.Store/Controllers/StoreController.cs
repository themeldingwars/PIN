using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebHost.Store.Controllers {
	[ApiController]
	public class StoreController : ControllerBase {
		private readonly ILogger<StoreController> _logger;

		public StoreController( ILogger<StoreController> logger ) {
			_logger = logger;
		}

		[HttpGet]
		[Route("")]
		public ActionResult Index() {
			return Ok();
		}
	}
}