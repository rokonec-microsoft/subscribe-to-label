using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNet.SubscribeToLabel.Web.Features.Repositories
{
    public interface IListRepositories
    {
        Task<IReadOnlyCollection<RepositoryModel>> GetAllRepositories();
    }
}
