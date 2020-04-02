using System.Threading.Tasks;
using Octokit;

namespace DotNet.SubscribeToLabel.Web.Features.GitHubApi
{
    public interface IGitHubClientFactory
    {
        /// <summary>
        /// This client has only acess to GitHub application API, for API like Issues or PullRequest
        /// see <see cref="GetGitHubInstallationClient(int)"/>
        /// </summary>
        IGitHubClient GetGitHubClient();

        /// <summary>
        /// Installation client is the GitHubClient you will mostly need as only
        /// this client has access to resources granted to GitHub app installation
        /// </summary>
        Task<IGitHubClient> GetGitHubInstallationClient();


        /// <summary>
        /// Installation client is the GitHubClient you will mostly need as only
        /// this client has access to resources granted to particular GitHub app installation
        /// TODO: consider to delete
        /// </summary>
        Task<IGitHubClient> GetGitHubInstallationClient(int installationId);
    }
}