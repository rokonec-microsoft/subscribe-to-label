using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using DotNet.SubscribeToLabel.Web.Features.IssueSubscriptions;
using DotNet.SubscribeToLabel.Web.Features.LabelSubscriptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DotNet.SubscribeToLabel.Tests.IssueSubscriptionTests
{
    public class Given_no_issue_labeled
    {
        public Given_no_issue_labeled()
        {
            UserLabelRepository = Substitute.For<InMemoryLabelSubscriptionRepository>();
            IssueSubscriber = Substitute.For<IIssueSubscriber>();
            IssueQuery = Substitute.For<IIssueQuery>();
            IssueSubscription = new IssueSubscription(new InMemoryIssueLabelRepository(), UserLabelRepository, IssueSubscriber);
            LabelSubscription = new LabelSubscription(UserLabelRepository, IssueSubscriber, IssueQuery);
        }

        protected DateTime ToBeLabeledAt { get; } = DateTime.Parse("2020-02-02", CultureInfo.InvariantCulture);

        protected IssueReference Issue => new IssueReference(RepositoryOwner, RepositoryName, 1, 111, "X+");

        protected const string Label = "label-x";
        protected const string AreaLabel = "area-51";
        protected const string RepositoryOwner = "ownerX";
        protected const string RepositoryName = "repoY";

        public ILabelSubscriptionRepository UserLabelRepository { get; }
        public IIssueSubscriber IssueSubscriber { get; }
        public IIssueQuery IssueQuery { get; set; }
        public ILabelSubscription LabelSubscription { get; set; }
        protected IssueSubscription IssueSubscription { get; }
    }

    public class Given_issue_labeled : Given_no_issue_labeled
    {
        public Given_issue_labeled()
        {
            IssueSubscription.TryAddLabel(Issue, Label, AlreadyLabeledAt).Wait();
        }

        protected DateTime AlreadyLabeledAt { get; } = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture);
    }

    public class Given_issue_area_label : Given_no_issue_labeled
    {
        public Given_issue_area_label()
        {
            IssueSubscription.TryAddLabel(Issue, AreaLabel, AlreadyLabeledAt).Wait();
        }

        protected DateTime AlreadyLabeledAt { get; } = DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture);
    }

    public class Given_issue_area_label_removed : Given_issue_area_label
    {
        public Given_issue_area_label_removed()
        {
            AlreadyRemovedAt = AlreadyLabeledAt.AddMinutes(+1);
            IssueSubscription.TryRemoveLabel(Issue, Label, AlreadyRemovedAt).Wait();
        }

        protected DateTime AlreadyRemovedAt { get; }
    }

    public class Given_issue_had_label_removed : Given_issue_labeled
    {
        public Given_issue_had_label_removed()
        {
            AlreadyRemovedAt = AlreadyLabeledAt.AddMinutes(+1);
            IssueSubscription.TryRemoveLabel(Issue, Label, AlreadyRemovedAt).Wait();
        }

        protected DateTime AlreadyRemovedAt { get; }
    }

    public class New_issue_can : Given_no_issue_labeled
    {
        [Fact]
        public async Task handle_set_and_remove_in_reverse_order()
        {
            // with remove time after label
            var wasRemoved = await IssueSubscription.TryRemoveLabel(Issue, AreaLabel, ToBeLabeledAt.AddHours(1));
            // with label time before remove but executed after remove
            var wasAdded = await IssueSubscription.TryAddLabel(Issue, AreaLabel, ToBeLabeledAt);
            var labeled = await IssueSubscription.TestLabel(Issue, AreaLabel);

            wasRemoved.Should().BeTrue();
            wasAdded.Should().BeFalse();
            labeled.Should().BeFalse();
        }

        [Fact]
        public async Task set_area_label()
        {
            var wasAdded = await IssueSubscription.TryAddLabel(Issue, AreaLabel, ToBeLabeledAt);
            var labeled = await IssueSubscription.TestLabel(Issue, AreaLabel);

            wasAdded.Should().BeTrue();
            labeled.Should().BeTrue();
        }

        [Fact]
        public async Task set_label()
        {
            var wasAdded = await IssueSubscription.TryAddLabel(Issue, Label, ToBeLabeledAt);
            var labeled = await IssueSubscription.TestLabel(Issue, Label);

            wasAdded.Should().BeTrue();
            labeled.Should().BeTrue();
        }
    }

    public class Labeled_issue_can : Given_issue_labeled
    {
        [Fact]
        public async Task handle_idempotent_label()
        {
            var wasAdded = await IssueSubscription.TryAddLabel(Issue, Label, AlreadyLabeledAt);
            var labeled = await IssueSubscription.TestLabel(Issue, Label);

            wasAdded.Should().BeFalse();
            labeled.Should().BeTrue();
        }

        [Fact]
        public async Task handle_outdated_label()
        {
            var wasAdded = await IssueSubscription.TryAddLabel(Issue, Label, AlreadyLabeledAt.AddHours(-1));
            var labeled = await IssueSubscription.TestLabel(Issue, Label);

            // so we can handle situation when labeling events are received out of order
            wasAdded.Should().BeFalse();
            labeled.Should().BeTrue();
        }

        [Fact]
        public async Task handle_outdated_remove_label()
        {
            var wasAdded = await IssueSubscription.TryRemoveLabel(Issue, Label, AlreadyLabeledAt.AddHours(-1));
            var labeled = await IssueSubscription.TestLabel(Issue, Label);

            // so we can handle situation when labeling events are received out of order
            wasAdded.Should().BeFalse();
            labeled.Should().BeTrue();
        }

        [Fact]
        public async Task remove_label()
        {
            var wasRemoved = await IssueSubscription.TryRemoveLabel(Issue, Label, ToBeLabeledAt);
            var labeled = await IssueSubscription.TestLabel(Issue, Label);

            wasRemoved.Should().BeTrue();
            labeled.Should().BeFalse();
        }

        [Fact]
        public async Task remove_non_assigned_label()
        {
            var wasRemoved = await IssueSubscription.TryRemoveLabel(Issue, "non-existing-label", ToBeLabeledAt);
            var labeled = await IssueSubscription.TestLabel(Issue, "non-existing-label");

            wasRemoved.Should()
                .BeTrue(
                    "Removing not used label, make record that such issue has removed that label, so we can detect outdated labels");
            labeled.Should().BeFalse();
        }

        [Fact]
        public async Task set_another_label()
        {
            var wasAdded = await IssueSubscription.TryAddLabel(Issue, "another-label", ToBeLabeledAt);
            var labeled = await IssueSubscription.TestLabel(Issue, "another-label");

            wasAdded.Should().BeTrue();
            labeled.Should().BeTrue();
        }

        [Fact]
        public async Task set_to_same_label()
        {
            var wasAdded = await IssueSubscription.TryAddLabel(Issue, Label, ToBeLabeledAt);
            var labeled = await IssueSubscription.TestLabel(Issue, Label);

            wasAdded.Should().BeFalse();
            labeled.Should().BeTrue();
        }
    }

    public class Issue_with_area_label_can : Given_issue_area_label
    {
        [Fact]
        public async Task handle_idempotent_label()
        {
            var wasAdded = await IssueSubscription.TryAddLabel(Issue, AreaLabel, AlreadyLabeledAt);
            var labeled = await IssueSubscription.TestLabel(Issue, AreaLabel);

            wasAdded.Should().BeFalse();
            labeled.Should().BeTrue();

            // TODO: verify no comments was created or updated
        }

        [Fact]
        public async Task remove_area_label()
        {
            var wasRemoved = await IssueSubscription.TryRemoveLabel(Issue, AreaLabel, ToBeLabeledAt);
            var labeled = await IssueSubscription.TestLabel(Issue, AreaLabel);

            wasRemoved.Should().BeTrue();
            labeled.Should().BeFalse();
        }

        [Fact]
        public async Task remove_non_assigned_area_label()
        {
            var wasRemoved = await IssueSubscription.TryRemoveLabel(Issue, "area-non-existing", ToBeLabeledAt);
            var labeled = await IssueSubscription.TestLabel(Issue, "area-non-existing");

            wasRemoved.Should()
                .BeTrue(
                    "Removing not used label, make record that such issue has removed that label, so we can detect outdated labels");
            labeled.Should().BeFalse();
        }
    }

    public class Issue_with_cleared_area_label_can : Given_issue_area_label_removed
    {
        [Fact]
        public async Task change_area_label()
        {
            var wasAdded = await IssueSubscription.TryAddLabel(Issue, "area-another", ToBeLabeledAt);
            var labeled = await IssueSubscription.TestLabel(Issue, "area-another");

            wasAdded.Should().BeTrue();
            labeled.Should().BeTrue();
        }

        [Fact]
        public async Task subscribe_already_subscribed_user()
        {
            await LabelSubscription.SetUserSubscription(RepositoryOwner, RepositoryName, "userX", Label);

            IssueSubscriber.ClearReceivedCalls();
            await IssueSubscription.TryAddLabel(Issue, Label, ToBeLabeledAt);

            //assert
            await IssueSubscriber.ReceivedWithAnyArgs().SubscribeToIssue(default!, default!);
        }
    }

    public class Cleared_issue_can : Given_issue_had_label_removed
    {
        [Fact]
        public async Task handle_idempotent_label_remove()
        {
            var wasAdded = await IssueSubscription.TryRemoveLabel(Issue, Label, AlreadyRemovedAt);
            var labeled = await IssueSubscription.TestLabel(Issue, Label);

            wasAdded.Should().BeFalse();
            labeled.Should().BeFalse();
        }

        [Fact]
        public async Task ignore_outdated_label()
        {
            var wasAdded = await IssueSubscription.TryAddLabel(Issue, Label, AlreadyRemovedAt.AddHours(-1));
            var labeled = await IssueSubscription.TestLabel(Issue, Label);

            wasAdded.Should().BeFalse();
            labeled.Should().BeFalse();
        }

        [Fact]
        public async Task reassign_label()
        {
            var wasAdded = await IssueSubscription.TryAddLabel(Issue, Label, ToBeLabeledAt);
            var labeled = await IssueSubscription.TestLabel(Issue, Label);

            wasAdded.Should().BeTrue();
            labeled.Should().BeTrue();
        }
    }

    public class Any_issue_can : Given_no_issue_labeled
    {
        [Fact]
        public async Task subscribe_nobody_to_issue_labeled_to_label_without_subscription()
        {
            await IssueSubscription.TryAddLabel(Issue, Label, ToBeLabeledAt);

            // assert
            await IssueSubscriber.DidNotReceiveWithAnyArgs().SubscribeToIssue(
                default!,
                default!);
        }

        [Fact]
        public async Task subscribe_users_subscribed_to_label()
        {
            await LabelSubscription.SetUserSubscription(RepositoryOwner, RepositoryName, "userX", Label);
            await LabelSubscription.SetUserSubscription(RepositoryOwner, RepositoryName, "userY", Label);
            await IssueSubscription.TryAddLabel(Issue, Label, ToBeLabeledAt);

            // assert
            await IssueSubscriber.Received(1).SubscribeToIssue(
                Arg.Is<IssueReference>(x => x.Id == Issue.Id),
                Arg.Is<IReadOnlyCollection<LabelSubscriptionModel>>(x => x.Count == 2));
        }
    }

    public class Any_issue_cannot : Given_no_issue_labeled
    {
        [Theory]
        [InlineData(0, 1, "x")]
        [InlineData(1, 0, "x")]
        [InlineData(-1, 1, "x")]
        [InlineData(1, -1, "x")]
        [InlineData(int.MinValue, 1, "x")]
        [InlineData(1, int.MinValue, "x")]
        public void set_label_for_invalid_issue(int id, int number, string nodeId)
        {
            Func<Task> act = async () => await IssueSubscription.TryAddLabel(new IssueReference(RepositoryOwner, RepositoryName, number, id, nodeId), Label, ToBeLabeledAt);

            act.Should().Throw<ArgumentException>().WithMessage("*issue*");
        }

        [Theory]
        [InlineData("")]
        [InlineData(",")]
        [InlineData(",suf")]
        [InlineData("pref,")]
        [InlineData("pref,suf")]
        [InlineData("|")]
        [InlineData("pref|suf")]
        public void set_invalid_label(string invalidLabel)
        {
            Func<Task> act = async () => await IssueSubscription.TryAddLabel(Issue, invalidLabel, ToBeLabeledAt);

            act.Should().Throw<ArgumentException>().WithMessage("*label*");
        }

        [Fact]
        public void set_null_label()
        {
            Func<Task> act = async () => await IssueSubscription.TryAddLabel(Issue, null!, ToBeLabeledAt);

            act.Should().Throw<ArgumentNullException>().WithMessage("*label*");
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("userX", null)]
        [InlineData(null, "repoY")]
        [InlineData("userX", "")]
        [InlineData("", "repoY")]
        [InlineData("", "")]
        public async Task refer_invalid_repo(string repoOwner, string repoName)
        {
            Func<Task> actAddLabel = async () => await IssueSubscription.TryAddLabel(new IssueReference(repoOwner, repoName, 1, 123, "X+"), Label, ToBeLabeledAt);
            Func<Task> actRemoveLabel = async () => await IssueSubscription.TryRemoveLabel(new IssueReference(repoOwner, repoName, 1, 123, "X+"), Label, ToBeLabeledAt);

            await actAddLabel.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*repository*");
            await actRemoveLabel.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*repository*");
        }
    }
}
