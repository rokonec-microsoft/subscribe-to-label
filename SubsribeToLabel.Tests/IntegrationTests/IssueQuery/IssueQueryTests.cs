using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNet.SubscribeToLabel.Tests.IntegrationTests.GitHubNotifier;
using DotNet.SubscribeToLabel.Web.Features.LabelSubscriptions;
using FluentAssertions;
using Octokit;
using Xunit;

namespace DotNet.SubscribeToLabel.Tests.IntegrationTests.IssueQuery
{
    public class Using_github_issue_query : Using_integration_with_github_test_organization
    {
        public Using_github_issue_query()
        {
            GitHubIssueQuery = new GitHubIssueQuery(GitHubClientFactory);
        }

        protected GitHubIssueQuery GitHubIssueQuery { get; }
    }

    public class Given_no_labeled_issues : Using_github_issue_query
    {
    }

    public class Given_labeled_issues : Given_no_labeled_issues
    {
        protected IReadOnlyCollection<Issue> NoLabeledIssues { get; private set; } = null!;
        protected IReadOnlyCollection<Issue> CatLabeledIssues { get; private set; } = null!;
        protected IReadOnlyCollection<Issue> DogLabeledIssues { get; private set; } = null!;
        protected IReadOnlyCollection<Issue> CatAndDogLabeledIssues { get; private set; } = null!;
        protected IReadOnlyCollection<Issue> AllIssues { get; private set; } = null!;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            var allIssues = new List<Issue>();

            NoLabeledIssues = await CreateIssues(allIssues, 2);
            CatLabeledIssues = await CreateIssues(allIssues, 2, AreaCat);
            DogLabeledIssues = await CreateIssues(allIssues, 2, AreaDog);
            CatAndDogLabeledIssues = await CreateIssues(allIssues, 2, AreaCat, AreaDog);

            AllIssues = allIssues;
        }

        private async Task<IReadOnlyCollection<Issue>> CreateIssues(List<Issue> allIssues, int count, params string[] labels)
        {
            var result = new List<Issue>();

            for (int i = 0; i < count; i++)
            {
                var newIssue = new NewIssue($"IntTest-{Guid.NewGuid()}")
                {
                    Body = "## Integration Test\nCould be deleted",
                };
                foreach (var label in labels)
                {
                    newIssue.Labels.Add(label);
                }
                result.Add(await GitHubAppInstallationsClient.Issue.Create(RepositoryOwner, RepositoryName, newIssue));
            }

            allIssues.AddRange(result);

            return result;
        }

        public override async Task DisposeAsync()
        {
            foreach (var issue in AllIssues)
            {
                await CloseIssue(issue);
            }

            await base.DisposeAsync();
        }

        protected async Task CloseIssue(Issue issue)
        {
            var toUpdate = issue.ToUpdate();
            toUpdate.State = ItemState.Closed;
            await GitHubAppInstallationsClient.Issue.Update(RepositoryOwner, RepositoryName, issue.Number, toUpdate);
        }
    }

    public class Repository_can : Given_no_labeled_issues
    {
        [Fact]
        public async Task return_empty_list_for_non_existing_label()
        {
            var issues = await GitHubIssueQuery.SearchOpenIssuesWithLabel(RepositoryOwner, RepositoryName, "non-exiting-label");

            issues.Should().BeEmpty();
        }
    }

    public class Repository_cannot : Given_no_labeled_issues
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData("userX", null)]
        [InlineData(null, "repoY")]
        [InlineData("userX", "")]
        [InlineData("", "repoY")]
        [InlineData("", "")]
        public async Task refer_invalid_repo(string repoOwner, string repoName)
        {
            Func<Task> actAddLabel = async () => await GitHubIssueQuery.SearchOpenIssuesWithLabel(repoOwner, repoName, AreaCat);

            await actAddLabel.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*repository*");
        }

        [Theory]
        [InlineData("")]
        [InlineData(",")]
        [InlineData(",suf")]
        [InlineData("pref,")]
        [InlineData("pref,suf")]
        [InlineData("|")]
        [InlineData("pref|suf")]
        public void refer_invalid_label(string invalidLabel)
        {
            Func<Task> act = async () => await GitHubIssueQuery.SearchOpenIssuesWithLabel(RepositoryOwner, RepositoryName, invalidLabel);

            act.Should().Throw<ArgumentException>().WithMessage("*label*");
        }
    }

    public class Repository_with_labeled_issues_can : Given_labeled_issues
    {
        [Fact]
        public async Task return_empty_list_for_non_existing_label()
        {
            var issues = await GitHubIssueQuery.SearchOpenIssuesWithLabel(RepositoryOwner, RepositoryName, "non-exiting-label");

            issues.Should().BeEmpty();
        }

        [Fact]
        public async Task return_items_by_label()
        {
            var catsIssues = await GitHubIssueQuery.SearchOpenIssuesWithLabel(RepositoryOwner, RepositoryName, AreaCat);
            var dogsIssues = await GitHubIssueQuery.SearchOpenIssuesWithLabel(RepositoryOwner, RepositoryName, AreaDog);

            CatLabeledIssues.All(i => catsIssues.Any(si => si.Id == i.Id)).Should().BeTrue();
            DogLabeledIssues.All(i => dogsIssues.Any(si => si.Id == i.Id)).Should().BeTrue();
            CatAndDogLabeledIssues.All(i => dogsIssues.Any(si => si.Id == i.Id) || catsIssues.Any(si => si.Id == i.Id)).Should().BeTrue();
            NoLabeledIssues.Any(i => dogsIssues.Any(si => si.Id == i.Id) || catsIssues.Any(si => si.Id == i.Id)).Should().BeFalse();
        }
    }

    public class Repository_with_labeled_issues_cannot : Given_labeled_issues
    {
        [Fact]
        public async Task return_closed_issues()
        {
            // arrange
            var closedCatIssue = CatLabeledIssues.Last();
            await CloseIssue(closedCatIssue);

            // act
            var catsIssues = await GitHubIssueQuery.SearchOpenIssuesWithLabel(RepositoryOwner, RepositoryName, AreaCat);

            catsIssues.Should().NotBeEmpty();
            catsIssues.Should().NotContain(i => i.Id == closedCatIssue.Id);
        }

        [Fact]
        public async Task return_items_by_label()
        {
            var catsIssues = await GitHubIssueQuery.SearchOpenIssuesWithLabel(RepositoryOwner, RepositoryName, AreaCat);
            var dogsIssues = await GitHubIssueQuery.SearchOpenIssuesWithLabel(RepositoryOwner, RepositoryName, AreaDog);

            CatLabeledIssues.All(i => catsIssues.Any(si => si.Id == i.Id)).Should().BeTrue();
            DogLabeledIssues.All(i => dogsIssues.Any(si => si.Id == i.Id)).Should().BeTrue();
            CatAndDogLabeledIssues.All(i => dogsIssues.Any(si => si.Id == i.Id) || catsIssues.Any(si => si.Id == i.Id)).Should().BeTrue();
            NoLabeledIssues.Any(i => dogsIssues.Any(si => si.Id == i.Id) || catsIssues.Any(si => si.Id == i.Id)).Should().BeFalse();
        }
    }
}
