using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNet.SubscribeToLabel.Web.Features.GitHubApi;
using DotNet.SubscribeToLabel.Web.Features.IssueSubscriptions;
using DotNet.SubscribeToLabel.Web.Features.Shared;
using Octokit;

namespace DotNet.SubscribeToLabel.Web.Features.LabelSubscriptions
{
    public class GitHubIssueQuery : IIssueQuery
    {
        private readonly IGitHubClientFactory _gitHubClientFactory;

        public GitHubIssueQuery(IGitHubClientFactory gitHubClientFactory)
        {
            _gitHubClientFactory = gitHubClientFactory;
        }

        public async Task<IReadOnlyCollection<IssueReference>> SearchOpenIssuesWithLabel(string repositoryOwner, string repositoryName, string label)
        {
            Validation.ValidateLabel(label);
            Validation.ValidateRepository(repositoryOwner, repositoryName);

            // issue query for all projects for opened issues with label
            var installationClient = await _gitHubClientFactory.GetGitHubInstallationClient();

            var issues = await installationClient.Issue.GetAllForRepository(repositoryOwner, repositoryName, new RepositoryIssueRequest
            {
                State = ItemStateFilter.Open,
                Labels = { label },
                Filter = IssueFilter.All
            });

            return issues.Select(i => ToIssueReference(repositoryOwner, repositoryName, i)).ToList();
        }

        private static IssueReference ToIssueReference(string repositoryOwner, string repositoryName, Issue issue) 
            => new IssueReference(repositoryOwner, repositoryName, issue.Number, issue.Id, issue.NodeId);
    }
}