using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WebHost.OperatorApi.Capability;

namespace WebHost.OperatorApi.Controllers
{
    [ApiController]
    public class CapabilityController : ControllerBase
    {
        private readonly ICapabilityRepository _capabilityRepository;
        private readonly ILogger<CapabilityController> _logger;

        public CapabilityController(ILogger<CapabilityController> logger,
                                    ICapabilityRepository capabilityRepository)
        {
            _logger = logger;
            _capabilityRepository = capabilityRepository;
        }

        [HttpGet]
        [Route("check")]
        public async Task<HostInformation> CheckAsync(string environment, int build)
        {
            return await _capabilityRepository.GetHostInformationAsync(environment, build);
        }

        [HttpGet]
        [Route("api/v1/products/{productName}")]
        public async Task<ProductInformation> Products(string productName)
        {
            return await _capabilityRepository.GetProductInformationAsync(productName);
        }
    }
}