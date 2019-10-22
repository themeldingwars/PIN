using System.Threading.Tasks;
using Operator.Domain.Capability;
using Operator.Infrastructure.Exceptions;

namespace Operator.Infrastructure.Capability
{
    public class CapabilityRepository : ICapabilityRepository
    {
        public async Task<HostInformation> GetHostInformationAsync(string environment, int build)
        {
            return await Task.FromResult(new HostInformation
                                         {
                                             FrontendHost = "https://localhost:44305",
                                             StoreHost = "https://localhost:44306",
                                             ChatServer = "https://localhost:44307",
                                             ReplayHost = $"https://localhost:44308/{environment}-{build}",
                                             WebHost = "https://localhost:44309",
                                             MarketHost = "https://localhost:44310",
                                             IngameHost = "https://localhost:44303",
                                             ClientapiHost = "https://localhost:44302",
                                             WebAssetHost = "https://localhost:44301",
                                             WebAccountsHost = "https://localhost:44304",
                                             RhsigscanHost = "https://localhost:44311"
                                         });
        }

        public async Task<ProductInformation> GetProductInformationAsync(string productName)
        {
            if (productName != "Firefall_Beta")
            {
                throw new NotFoundException($"Product '{productName}' is unknown");
            }

            return await Task.FromResult(new ProductInformation
                                         {
                                             Build = "beta-1973",
                                             Environment = "production",
                                             Region = "NA",
                                             PatchLevel = 0
                                         });
        }
    }
}