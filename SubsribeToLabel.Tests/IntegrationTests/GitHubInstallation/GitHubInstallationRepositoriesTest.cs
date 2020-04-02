using System.Threading.Tasks;
using DotNet.SubscribeToLabel.Tests.IntegrationTests.GitHubNotifier;
using DotNet.SubscribeToLabel.Web.Features.AreaOwner;
using DotNet.SubscribeToLabel.Web.Features.Repositories;
using FluentAssertions;
using Octokit;
using Xunit;

namespace DotNet.SubscribeToLabel.Tests.IntegrationTests.GitHubInstallation
{
    public class List_repositories_can : Using_integration_with_github_test_organization
    {
        [Fact]
        public async Task List_all_repositories_with_granted_permissions()
        {
            var listRepositories = new GitHubInstallationRepositories(GitHubClientFactory);

            var repos = await listRepositories.GetAllRepositories();

            repos.Should().Contain(r => r.Owner == RepositoryOwner && r.Name == RepositoryName && r.Id > 0);
        }
    }
}