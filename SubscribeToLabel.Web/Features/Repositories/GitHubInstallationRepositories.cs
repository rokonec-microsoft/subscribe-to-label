using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNet.SubscribeToLabel.Web.Features.GitHubApi;

namespace DotNet.SubscribeToLabel.Web.Features.Repositories
{
    public class GitHubInstallationRepositories : IListRepositories
    {
        private readonly IGitHubClientFactory _gitHubClientFactory;

        public GitHubInstallationRepositories(IGitHubClientFactory gitHubClientFactory)
        {
            _gitHubClientFactory = gitHubClientFactory;
        }

        public async Task<IReadOnlyCollection<RepositoryModel>> GetAllRepositories()
        {
            var gitHubInstallationClient = await _gitHubClientFactory.GetGitHubInstallationClient();

            var repos = await gitHubInstallationClient.GitHubApps.Installation.GetAllRepositoriesForCurrent();

            return repos.Repositories.Select(r => new RepositoryModel(r.Id, r.Owner.Login, r.Name)).ToList();
        }
    }
}