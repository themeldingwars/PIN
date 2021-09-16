using System.Threading.Tasks;
using WebHost.OperatorApi.Exceptions;

namespace WebHost.OperatorApi.Capability
{
    public class CapabilityRepository : ICapabilityRepository
    {
        public async Task<HostInformation> GetHostInformationAsync(string environment, int build)
        {
            return await Task.FromResult(new HostInformation
                                         {
                                             FrontendHost = "https://localhost:44399",
                                             StoreHost = "https://localhost:44399",
                                             ChatServer = "https://localhost:44307",
                                             ReplayHost = $"https://localhost:44399/{environment}-{build}",
                                             WebHost = "https://localhost:44399",
                                             MarketHost = "https://localhost:44399",
                                             IngameHost = "https://localhost:44303",
                                             ClientapiHost = "https://localhost:44302",
                                             WebAssetHost = "https://localhost:44399",
                                             WebAccountsHost = "https://localhost:44399",
                                             RhsigscanHost = "https://localhost:44399"
                                         });
        }

        public async Task<ProductInformation> GetProductInformationAsync(string productName)
        {
            if (productName != "Firefall_Beta")
            {
                throw new NotFoundException($"Product '{productName}' is unknown");
            }

            return await Task.FromResult(new ProductInformation { Build = "beta-1973", Environment = "production", Region = "NA", PatchLevel = 0 });
        }
    }
}