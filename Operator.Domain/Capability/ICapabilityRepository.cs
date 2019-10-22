using System.Threading.Tasks;

namespace Operator.Domain.Capability
{
    public interface ICapabilityRepository
    {
        Task<HostInformation> GetHostInformationAsync(string environment, int build);
        Task<ProductInformation> GetProductInformationAsync(string productName);
    }
}
