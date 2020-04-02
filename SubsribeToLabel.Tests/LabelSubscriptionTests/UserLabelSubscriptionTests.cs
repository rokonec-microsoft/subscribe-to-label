using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNet.SubscribeToLabel.Web.Features.IssueSubscriptions;
using DotNet.SubscribeToLabel.Web.Features.LabelSubscriptions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DotNet.SubscribeToLabel.Tests.LabelSubscriptionTests
{
    public abstract class Given_no_label_subscription
    {
        protected Given_no_label_subscription()
        {
            IssueSubscriber = Substitute.For<IIssueSubscriber>();
            IssueQuery = Substitute.For<IIssueQuery>();
            LabelSubscription = new LabelSubscription(new InMemoryLabelSubscriptionRepository(), IssueSubscriber, IssueQuery);
        }

        protected string UserId { get; } = "rokonec";

        protected IIssueSubscriber IssueSubscriber { get; }
        public IIssueQuery IssueQuery { get; set; }
        protected ILabelSubscription LabelSubscription { get; }
    }

    public abstract class Give_only_other_users_have_subscriptions : Given_no_label_subscription
    {
        protected const string OtherUser = "otherUser";
        protected const string AnotherUser = "anotherUser";
        protected const string RepoOwner = "ownerX";
        protected const string RepoName = "repoY";

        protected Give_only_other_users_have_subscriptions()
        {
            LabelSubscription.SetUserSubscription(RepoOwner, RepoName, OtherUser, "Bug");
            LabelSubscription.SetUserSubscription(RepoOwner, RepoName, OtherUser, "Wonder");
            LabelSubscription.SetUserSubscription(RepoOwner, RepoName, AnotherUser, "Bug");
            LabelSubscription.SetUserSubscription(RepoOwner, RepoName, AnotherUser, "Invalid");
        }
    }

    public abstract class Given_user_have_subscriptions : Give_only_other_users_have_subscriptions
    {
        protected Given_user_have_subscriptions()
        {
            LabelSubscription.SetUserSubscription(RepoOwner, RepoName, UserId, "Bug");
            LabelSubscription.SetUserSubscription(RepoOwner, RepoName, UserId, "Wonder");
            LabelSubscription.SetUserSubscription(RepoOwner, RepoName, UserId, "Invalid");
        }
    }

    public class New_user_can : Give_only_other_users_have_subscriptions
    {
        [Fact]
        public async Task add_label()
        {
            LabelSubscriptionModel inserted = await LabelSubscription.SetUserSubscription(RepoOwner, RepoName, UserId, "Bug");

            inserted.Should().Match<LabelSubscriptionModel>(l => l.Label == "Bug" && l.UserId == UserId);
        }

        [Fact]
        public async Task add_labels()
        {
            // act
            IEnumerable<LabelSubscriptionModel> list = await LabelSubscription.SetUserSubscriptions(RepoOwner, RepoName, UserId, new [] { "Bug", "Wonder" });

            list.Should().NotBeNull()
                .And.HaveCount(2)
                .And.Contain(o => o.Label == "Bug" && o.UserId == UserId)
                .And.Contain(o => o.Label == "Wonder" && o.UserId == UserId);
        }

        [Fact]
        public async Task add_label_then_list_it()
        {
            // act
            await LabelSubscription.SetUserSubscription(RepoOwner, RepoName, UserId, "Bug");
            IEnumerable<LabelSubscriptionModel> list = await LabelSubscription.GetUserSubscriptions(RepoOwner, RepoName, UserId);

            list.Should().NotBeNull()
                .And.Contain(o => o.Label == "Bug" && o.UserId == UserId);
        }

        [Fact]
        public async Task add_labels_then_list_them()
        {
            // act
            await LabelSubscription.SetUserSubscriptions(RepoOwner, RepoName, UserId, new[] { "Bug", "Wonder" });
            IEnumerable<LabelSubscriptionModel> list = await LabelSubscription.GetUserSubscriptions(RepoOwner, RepoName, UserId);

            list.Should().NotBeNull()
                .And.HaveCount(2)
                .And.Contain(o => o.Label == "Bug" && o.UserId == UserId)
                .And.Contain(o => o.Label == "Wonder" && o.UserId == UserId);
        }

        [Fact]
        public async Task get_empty_list_of_labels()
        {
            // act
            IEnumerable<LabelSubscriptionModel> list = await LabelSubscription.GetUserSubscriptions(RepoOwner, RepoName, UserId);

            list.Should().NotBeNull().And.BeEmpty();
        }
    }

    public class User_cannot : Give_only_other_users_have_subscriptions
    {
        [Fact]
        public void anonymously_add_label()
        {
            Func<Task> act = async () => await LabelSubscription.SetUserSubscription(RepoOwner, RepoName, null!, "Bug");

            act.Should().Throw<ArgumentNullException>()
                .WithMessage("*userId*");
        }

        [Fact]
        public void anonymously_add_labels()
        {
            Func<Task> act = async () => await LabelSubscription.SetUserSubscriptions(RepoOwner, RepoName, null!, new[] { "Bug", "Wonder" });

            act.Should().Throw<ArgumentNullException>()
                .WithMessage("*userId*");
        }

        [Fact]
        public void add_null_label()
        {
            Func<Task> act = async () => await LabelSubscription.SetUserSubscription(RepoOwner, RepoName, UserId, null!);

            act.Should().Throw<ArgumentNullException>()
                .WithMessage("*label*");
        }

        [Fact]
        public void add_any_null_label()
        {
            Func<Task> act = async () => await LabelSubscription.SetUserSubscriptions(RepoOwner, RepoName, UserId, new[] { "Bug", null! });

            act.Should().Throw<ArgumentNullException>()
                .WithMessage("*label*");
        }

        [Fact]
        public async Task see_other_users_labels()
        {
            IEnumerable<LabelSubscriptionModel> list = await LabelSubscription.GetUserSubscriptions(RepoOwner, RepoName, UserId);
            IEnumerable<LabelSubscriptionModel> otherUserList = await LabelSubscription.GetUserSubscriptions(RepoOwner, RepoName, OtherUser);

            list.Should().NotBeNull().And.BeEmpty();
            otherUserList.Should().NotBeNull().And.NotBeEmpty();
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
            Func<Task> actSetSubscription = async () => await LabelSubscription.SetUserSubscription(repoOwner, repoName, UserId, "Bug");
            Func<Task> actSetUserSubscriptions = async () => await LabelSubscription.SetUserSubscriptions(repoOwner, repoName, UserId, new []{"Bug", "Wonder"});
            Func<Task> actGetUserSubscriptions = async () => await LabelSubscription.GetUserSubscriptions(repoOwner, repoName, UserId);

            await actSetSubscription.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*repository*");
            await actSetUserSubscriptions.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*repository*");
            await actGetUserSubscriptions.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*repository*");
        }
    }

    public class Returning_givenUser_can : Given_user_have_subscriptions
    {
        [Fact]
        public async Task list_labels()
        {
            IEnumerable<LabelSubscriptionModel> list = await LabelSubscription.GetUserSubscriptions(RepoOwner, RepoName, UserId);

            list.Should().NotBeNull()
                .And.HaveCount(3)
                .And.Contain(o => o.UserId == UserId && o.Label == "Bug")
                .And.Contain(o => o.UserId == UserId && o.Label == "Wonder")
                .And.Contain(o => o.UserId == UserId && o.Label == "Invalid");
        }

        [Fact]
        public async Task add_new_label()
        {
            LabelSubscriptionModel inserted = await LabelSubscription.SetUserSubscription(RepoOwner, RepoName, UserId, "x");

            // assert
            IEnumerable<LabelSubscriptionModel> list = await LabelSubscription.GetUserSubscriptions(RepoOwner, RepoName, UserId);

            inserted.Should().Match<LabelSubscriptionModel>(o => o.Label == "x" && o.UserId == UserId);
            list.Should().NotBeNull()
                .And.HaveCountGreaterThan(1)
                .And.Contain(o => o.UserId == UserId && o.Label == "x");
        }

        [Fact]
        public async Task change_list_of_labels_to_add_labels()
        {
            IEnumerable<LabelSubscriptionModel> existing = await LabelSubscription.GetUserSubscriptions(RepoOwner, RepoName, UserId);

            // act
            IEnumerable<LabelSubscriptionModel> list = await LabelSubscription.SetUserSubscriptions(RepoOwner, RepoName, UserId, 
                existing.Select(o => o.Label).Concat(new[] { "x", "y" }).ToList());

            list.Should().NotBeNull()
                .And.HaveCount(5)
                .And.Contain(o => o.Label == "x" && o.UserId == UserId)
                .And.Contain(o => o.Label == "y" && o.UserId == UserId);
        }

        [Fact]
        public async Task change_list_of_labels_to_remove_labels()
        {
            IEnumerable<LabelSubscriptionModel> existing = await LabelSubscription.GetUserSubscriptions(RepoOwner, RepoName, UserId);

            // act
            IEnumerable<LabelSubscriptionModel> list = await LabelSubscription.SetUserSubscriptions(RepoOwner, RepoName, UserId,
                existing.Skip(1).Select(o => o.Label).ToList());

            list.Should().NotBeNull()
                .And.HaveCount(existing.Count()-1);
        }

        [Fact]
        public async Task delete_all_his_labels()
        {
            IEnumerable<LabelSubscriptionModel> list = await LabelSubscription.SetUserSubscriptions(RepoOwner, RepoName, UserId, Array.Empty<string>());

            list.Should().NotBeNull()
                .And.BeEmpty();
        }

        [Fact]
        public async Task change_list_of_labels_to_add_and_remove_labels()
        {
            IEnumerable<LabelSubscriptionModel> existing = await LabelSubscription.GetUserSubscriptions(RepoOwner, RepoName, UserId);

            // act
            IEnumerable<LabelSubscriptionModel> list = await LabelSubscription.SetUserSubscriptions(RepoOwner, RepoName, UserId,
                existing.Skip(1).Select(o => o.Label).Concat(new[] { "x", "y" }).ToList());

            list.Should().NotBeNull()
                .And.HaveCount(existing.Count() - 1 + 2)
                .And.Contain(o => o.Label == "x" && o.UserId == UserId)
                .And.Contain(o => o.Label == "y" && o.UserId == UserId);
        }
    }

    public class Returning_user_cannot : Given_user_have_subscriptions
    {
        [Fact]
        public async Task see_other_user_labels()
        {
            IEnumerable<LabelSubscriptionModel> list = await LabelSubscription.GetUserSubscriptions(RepoOwner, RepoName, UserId);

            list.Should().NotBeNull()
                .And.NotBeEmpty()
                .And.NotContain(o => o.UserId != UserId);
        }

        [Fact]
        public async Task add_labels_to_other_user()
        {
            IEnumerable<LabelSubscriptionModel> listOther = await LabelSubscription.GetUserSubscriptions(RepoOwner, RepoName, OtherUser);
            IEnumerable<LabelSubscriptionModel> listAnother = await LabelSubscription.GetUserSubscriptions(RepoOwner, RepoName, AnotherUser);
            var otherCount = listOther.Count() + listAnother.Count();

            // act
            await LabelSubscription.SetUserSubscriptions(RepoOwner, RepoName, UserId, new[] { "x", "y" });

            // assert
            listOther = await LabelSubscription.GetUserSubscriptions(RepoOwner, RepoName, OtherUser);
            listAnother = await LabelSubscription.GetUserSubscriptions(RepoOwner, RepoName, AnotherUser);
            var otherCountAfter = listOther.Count() + listAnother.Count();

            otherCount.Should().Be(otherCountAfter);
            listOther.Should().NotContain(o => o.Label == "x" || o.Label == "y");
            listAnother.Should().NotContain(o => o.Label == "x" || o.Label == "y");
        }

        [Fact] public async Task delete_labels_of_other_user()
        {
            IEnumerable<LabelSubscriptionModel> listOther = await LabelSubscription.GetUserSubscriptions(RepoOwner, RepoName, OtherUser);
            IEnumerable<LabelSubscriptionModel> listAnother = await LabelSubscription.GetUserSubscriptions(RepoOwner, RepoName, AnotherUser);
            var otherCount = listOther.Count() + listAnother.Count();

            // act
            await LabelSubscription.SetUserSubscriptions(RepoOwner, RepoName, UserId, Array.Empty<string>());

            // assert
            listOther = await LabelSubscription.GetUserSubscriptions(RepoOwner, RepoName, OtherUser);
            listAnother = await LabelSubscription.GetUserSubscriptions(RepoOwner, RepoName, AnotherUser);
            var otherCountAfter = listOther.Count() + listAnother.Count();

            otherCount.Should().Be(otherCountAfter);
        }
    }
}
