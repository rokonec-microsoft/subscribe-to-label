using System.Threading.Tasks;

namespace DotNet.SubscribeToLabel.Web.Features.AreaOwner
{
    public interface IAreaOwnerProvider
    {
        Task<AreaOwnerModel> Get(string repositoryOwner, string repositoryName);
    }
}
