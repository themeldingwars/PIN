using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebHost.Replay.Controllers
{
    [ApiController]
    public class ReplayController : ControllerBase
    {
        private readonly ILogger<ReplayController> _logger;

        public ReplayController(ILogger<ReplayController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("")]
        public ActionResult Index()
        {
            return Ok();
        }
    }
}