using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DotNet.SubscribeToLabel.Web.Features.GitHubApi;
using DotNet.SubscribeToLabel.Web.Features.IssueSubscriptions;
using DotNet.SubscribeToLabel.Web.Features.LabelSubscriptions;
using DotNet.SubscribeToLabel.Web.Models.Settings;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NSubstitute;
using Octokit;
using Xunit;

namespace DotNet.SubscribeToLabel.Tests.IntegrationTests.GitHubNotifier
{
    public class Using_integration_with_github_test_organization : IAsyncLifetime
    {
        IConfiguration Configuration { get; set; }

        public Using_integration_with_github_test_organization()
        {
            var options = Substitute.For<IOptions<GitHubAppOptions>>();

            var builder = new ConfigurationBuilder()
                .AddUserSecrets<Using_integration_with_github_test_organization>();

            Configuration = builder.Build();

            var gitHubAppOptions = new GitHubAppOptions
            {
                ApplicationId = 0,
                InstallationId = 0,
                Name = "Subscribe_to_Label",
                PrivateKey = new PrivateKeyOptions
                {
                    KeyString = null,
                }
            };

            Configuration.Bind("GitHubApp", gitHubAppOptions);

            options.Value.Returns(gitHubAppOptions);

            RepositoryOwner = "rokonec-int-tests";
            RepositoryName = "issue-notify-tests";
            UserX = "TestLabelNotifyUserX";
            UserY = "TestLabelNotifyUserY";
            AreaCat = "area-cat";
            AreaDog = "area-dog";

            GitHubClientFactory = new GitHubClientFactory(options);
        }

        public string UserY { get; }

        public string UserX { get; }

        protected IGitHubClient GitHubAppInstallationsClient { get; private set; } = null!;

        protected GitHubClientFactory GitHubClientFactory { get; }

        protected string RepositoryName { get; set; }

        protected string RepositoryOwner { get; set; }

        protected string AreaDog { get; }
        protected string AreaCat { get; }

        public virtual async Task InitializeAsync()
        {
            GitHubAppInstallationsClient = await GitHubClientFactory.GetGitHubInstallationClient();
        }

        public virtual Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }

    public class Using_integration_github_test_subscriber : Using_integration_with_github_test_organization
    {
        public Using_integration_github_test_subscriber()
        {
            GitHubIssueSubscriber = new GitHubIssueSubscriber(GitHubClientFactory);
        }

        public GitHubIssueSubscriber GitHubIssueSubscriber { get; }
    }

    public class Given_new_issue : Using_integration_github_test_subscriber
    {
        protected Issue Issue { get; private set; } = null!;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            Issue = await GitHubAppInstallationsClient.Issue.Create(RepositoryOwner, RepositoryName,
                new NewIssue($"IntTest-{Guid.NewGuid()}")
                {
                    Body = "## Integration Test\nCould be deleted"
                });
        }

        public override async Task DisposeAsync()
        {
            var toUpdate = Issue.ToUpdate();
            toUpdate.State = ItemState.Closed;
            await GitHubAppInstallationsClient.Issue.Update(RepositoryOwner, RepositoryName, Issue.Number, toUpdate);

            await base.DisposeAsync();
        }

        protected IssueReference IssueReference()
        {
            var issueReference = new IssueReference(RepositoryOwner, RepositoryName, Issue.Number, Issue.Id, Issue.NodeId);
            return issueReference;
        }

        protected async Task<IReadOnlyList<IssueEvent>> AssertUserIsSubscribed(string user)
        {
            var sw = Stopwatch.StartNew();
            IReadOnlyList<IssueEvent> events;
            do
            {
                events = await GitHubAppInstallationsClient.Issue.Events.GetAllForIssue(RepositoryOwner, RepositoryName, Issue.Number);

                try
                {
                    events.Should().Contain(e => e.Event == EventInfoState.Subscribed && e.Actor.Login == user);
                    break;
                }
                catch (Exception) when (sw.Elapsed < TimeSpan.FromMinutes(5)) // give it max 5 minutes to generate event records)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                }
            } while (sw.Elapsed < TimeSpan.FromMinutes(5)); // give it max 5 minutes to generate event records

            return events;
        }
    }

    public class Given_issue_with_subscription : Given_new_issue
    {
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            var issueReference = IssueReference();
            await GitHubIssueSubscriber.SubscribeToIssue(issueReference, new[]
            {
                new LabelSubscriptionModel(RepositoryOwner, RepositoryName, UserY, "area-dog"),
                new LabelSubscriptionModel(RepositoryOwner, RepositoryName, "another-userX", "area-cat"),
                new LabelSubscriptionModel(RepositoryOwner, RepositoryName, UserY, "area-cat"),
            });
        }
    }

    public class New_issue_can : Given_new_issue
    {
        [Fact]
        public async Task subscribe_new_user()
        {
            var issueReference = IssueReference();
            await GitHubIssueSubscriber.SubscribeToIssue(issueReference, new[]
            {
                new LabelSubscriptionModel(RepositoryOwner, RepositoryName, UserX, "area-cat")
            });

            await AssertUserIsSubscribed(UserX);
        }

        [Fact]
        public async Task subscribe_new_users_for_different_label()
        {
            var issueReference = IssueReference();
            await GitHubIssueSubscriber.SubscribeToIssue(issueReference, new[]
            {
                new LabelSubscriptionModel(RepositoryOwner, RepositoryName, UserX, "area-cat"),
                new LabelSubscriptionModel(RepositoryOwner, RepositoryName, UserY, "area-dog"),
            });

            await AssertUserIsSubscribed(UserX);
            await AssertUserIsSubscribed(UserY);
        }

        [Fact]
        public async Task subscribe_new_users_for_same_label()
        {
            var issueReference = IssueReference();
            await GitHubIssueSubscriber.SubscribeToIssue(issueReference, new[]
            {
                new LabelSubscriptionModel(RepositoryOwner, RepositoryName, UserX, "area-cat"),
                new LabelSubscriptionModel(RepositoryOwner, RepositoryName, UserY, "area-cat"),
            });

            await AssertUserIsSubscribed(UserX);
            await AssertUserIsSubscribed(UserY);
        }

        [Fact]
        public async Task unsubscribe_a_user()
        {
            var issueReference = IssueReference();
            await GitHubIssueSubscriber.UnsubscribeFromIssue(issueReference, new[]
            {
                new LabelSubscriptionModel(RepositoryOwner, RepositoryName, UserX, "area-cat"),
                new LabelSubscriptionModel(RepositoryOwner, RepositoryName, UserY, "area-dog"),
            });
        }
    }

    public class Issue_with_subscription_can : Given_issue_with_subscription
    {
        [Fact]
        public async Task subscribe_user_twice()
        {
            var issueReference = IssueReference();
            await GitHubIssueSubscriber.SubscribeToIssue(issueReference, new[]
            {
                new LabelSubscriptionModel(RepositoryOwner, RepositoryName, UserY, "area-dog")
            });
        }

        [Fact]
        public async Task unsubscribe_a_user()
        {
            var issueReference = IssueReference();
            await GitHubIssueSubscriber.SubscribeToIssue(issueReference, new[]
            {
                new LabelSubscriptionModel(RepositoryOwner, RepositoryName, UserY, "area-dog")
            });
        }

        [Fact]
        public async Task unsubscribe_a_user_twice()
        {
            var issueReference = IssueReference();
            await GitHubIssueSubscriber.SubscribeToIssue(issueReference, new[]
            {
                new LabelSubscriptionModel(RepositoryOwner, RepositoryName, UserY, "area-dog")
            });
            await GitHubIssueSubscriber.SubscribeToIssue(issueReference, new[]
            {
                new LabelSubscriptionModel(RepositoryOwner, RepositoryName, UserY, "area-dog")
            });
        }

        [Fact]
        public async Task subscribe_another_users_for_used_label()
        {
            var issueReference = IssueReference();
            await GitHubIssueSubscriber.SubscribeToIssue(issueReference, new[]
            {
                new LabelSubscriptionModel(RepositoryOwner, RepositoryName, UserX, "area-cat")
            });

            await AssertUserIsSubscribed(UserX);
        }

        [Fact]
        public async Task subscribe_another_users_for_new_label()
        {
            var issueReference = IssueReference();
            await GitHubIssueSubscriber.SubscribeToIssue(issueReference, new[]
            {
                new LabelSubscriptionModel(RepositoryOwner, RepositoryName, UserX, "area-fg576")
            });

            await AssertUserIsSubscribed(UserX);
        }
    }
}