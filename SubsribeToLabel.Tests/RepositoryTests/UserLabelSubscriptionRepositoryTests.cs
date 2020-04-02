using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet.SubscribeToLabel.Web.Features.LabelSubscriptions;
using FluentAssertions;
using Xunit;

namespace DotNet.SubscribeToLabel.Tests.RepositoryTests
{
    public class InMemoryUserLabelSubscriptionRepositoryTests
    {
        protected const string RepoOwner = "ownerX";
        protected const string RepoName = "repoY";
        protected const string UserId = "userX";

        private ILabelSubscriptionRepository Repository { get; } = new InMemoryLabelSubscriptionRepository();

        [Fact]
        public async Task GetUserLabel_NonExisting_Null()
        {
            LabelSubscriptionModel? userLabel = await Repository.GetSubscription(RepoOwner, RepoName, UserId, "Bug");

            userLabel.Should().BeNull();
        }

        [Fact]
        public async Task SetUserLabel_NonExisting_GetGetsIt()
        {
            LabelSubscriptionModel inserted = await Repository.SetSubscription(RepoOwner, RepoName, UserId, "Bug");
            LabelSubscriptionModel? retrieved = await Repository.GetSubscription(RepoOwner, RepoName, UserId, "Bug");

            retrieved.Should().NotBeNull();
            inserted.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateUserLabel_Existing_Updated()
        {
            LabelSubscriptionModel inserted = await Repository.SetSubscription(RepoOwner, RepoName, UserId, "Bug");

            // act
            LabelSubscriptionModel updated = await Repository.SetSubscription(RepoOwner, RepoName, UserId, "Bug");
            LabelSubscriptionModel? retrieved = await Repository.GetSubscription(RepoOwner, RepoName, UserId, "Bug");

            retrieved.Should().NotBeNull();
            updated.Should().NotBeNull();
            inserted.Should().NotBeNull();
        }

        [Fact]
        public async Task GetUserLabels_NonExisting_Empty()
        {
            IEnumerable<LabelSubscriptionModel> userLabels = await Repository.GetUserSubscriptions(RepoOwner, RepoName, UserId);

            userLabels.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public async Task GetUserLabels_Single_Single()
        {
            await Repository.SetSubscription(RepoOwner, RepoName, UserId, "Bug");

            // act
            IEnumerable<LabelSubscriptionModel> userLabels = await Repository.GetUserSubscriptions(RepoOwner, RepoName, UserId);

            userLabels.Should().NotBeNull()
                .And.Contain(l => l.Label == "Bug" && l.UserId == UserId);
        }

        [Fact]
        public async Task GetUserLabels_MultipleUsers_Single()
        {
            await Repository.SetSubscription(RepoOwner, RepoName, UserId, "Bug");
            await Repository.SetSubscription(RepoOwner, RepoName, "otherUser", "Bug");

            // act
            IEnumerable<LabelSubscriptionModel> userLabels = await Repository.GetUserSubscriptions(RepoOwner, RepoName, UserId);

            userLabels.Should().NotBeNull()
                .And.Contain(l => l.Label == "Bug" && l.UserId == UserId);
        }

        [Fact]
        public async Task GetUserLabels_MultipleLabels_AllReturned()
        {
            await Repository.SetSubscription(RepoOwner, RepoName, UserId, "Bug");
            await Repository.SetSubscription(RepoOwner, RepoName, UserId, "Critical");
            await Repository.SetSubscription(RepoOwner, RepoName, UserId, "Wonder");

            await Repository.SetSubscription(RepoOwner, RepoName, "otherUser", "Wonder");

            // act
            IEnumerable<LabelSubscriptionModel> userLabels = await Repository.GetUserSubscriptions(RepoOwner, RepoName, UserId);

            userLabels.Should().NotBeNull()
                .And.HaveCount(3)
                .And.Contain(l => l.Label == "Bug" && l.UserId == UserId)
                .And.Contain(l => l.Label == "Critical" && l.UserId == UserId)
                .And.Contain(l => l.Label == "Wonder" && l.UserId == UserId);
        }

        [Fact]
        public async Task GetUserLabels_OtherUsers_Empty()
        {
            await Repository.SetSubscription(RepoOwner, RepoName, "otherUser", "Bug");

            // act
            IEnumerable<LabelSubscriptionModel> userLabels = await Repository.GetUserSubscriptions(RepoOwner, RepoName, UserId);

            userLabels.Should().NotBeNull().And.BeEmpty();
        }

        /// <summary>
        /// ////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task GetLabelUsers_NonExisting_Empty()
        {
            IEnumerable<LabelSubscriptionModel> labelUsers = await Repository.GetLabelSubscriptions(RepoOwner, RepoName, "Bug");

            labelUsers.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public async Task GetLabelUsers_Single_Single()
        {
            await Repository.SetSubscription(RepoOwner, RepoName, UserId, "Bug");

            // act
            IEnumerable<LabelSubscriptionModel> labelUsers = await Repository.GetLabelSubscriptions(RepoOwner, RepoName, "Bug");

            labelUsers.Should().NotBeNull()
                .And.Contain(l => l.Label == "Bug" && l.UserId == UserId);
        }

        [Fact]
        public async Task GetLabelUsers_MultipleUsers_Two()
        {
            await Repository.SetSubscription(RepoOwner, RepoName, UserId, "Bug");
            await Repository.SetSubscription(RepoOwner, RepoName, "otherUser", "Bug");

            // act
            IEnumerable<LabelSubscriptionModel> labelUsers = await Repository.GetLabelSubscriptions(RepoOwner, RepoName, "Bug");

            labelUsers.Should().NotBeNull()
                .And.HaveCount(2)
                .And.Contain(l => l.Label == "Bug" && l.UserId == UserId)
                .And.Contain(l => l.Label == "Bug" && l.UserId == "otherUser");
        }

        [Fact]
        public async Task GetLabelUsers_MultipleLabels_One()
        {
            await Repository.SetSubscription(RepoOwner, RepoName, UserId, "Bug");
            await Repository.SetSubscription(RepoOwner, RepoName, UserId, "Critical");
            await Repository.SetSubscription(RepoOwner, RepoName, UserId, "Wonder");

            // act
            IEnumerable<LabelSubscriptionModel> labelUsers = await Repository.GetLabelSubscriptions(RepoOwner, RepoName, "Bug");

            labelUsers.Should().NotBeNull()
                .And.HaveCount(1)
                .And.Contain(l => l.Label == "Bug" && l.UserId == UserId);
        }

        [Fact]
        public async Task GetLabelUsers_OtherLabels_Empty()
        {
            await Repository.SetSubscription(RepoOwner, RepoName, UserId, "Critical");
            await Repository.SetSubscription(RepoOwner, RepoName, UserId, "Wonder");

            // act
            IEnumerable<LabelSubscriptionModel> labelUsers = await Repository.GetLabelSubscriptions(RepoOwner, RepoName, "Bug");

            labelUsers.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public async Task DeleteUserLabel_NotExisting_False()
        {
            bool result = await Repository.DeleteSubscription(RepoOwner, RepoName, UserId, "Critical");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteUserLabel_Existing_True()
        {
            await Repository.SetSubscription(RepoOwner, RepoName, UserId, "Critical");

            // act
            bool result = await Repository.DeleteSubscription(RepoOwner, RepoName, UserId, "Critical");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteUserLabel_Existing_Deleted()
        {
            await Repository.SetSubscription(RepoOwner, RepoName, UserId, "Bug");
            await Repository.SetSubscription(RepoOwner, RepoName, UserId, "Critical");
            await Repository.DeleteSubscription(RepoOwner, RepoName, UserId, "Critical");

            // act
            IEnumerable<LabelSubscriptionModel> userLabels = await Repository.GetUserSubscriptions(RepoOwner, RepoName, UserId);
            LabelSubscriptionModel? userLabel = await Repository.GetSubscription(RepoOwner, RepoName, UserId, "Critical");

            userLabels.Should().HaveCount(1);
            userLabel.Should().BeNull();
        }

        [Fact]
        public async Task SetForDifferentRepos_Empty_Pass()
        {
            await Repository.SetSubscription(RepoOwner, RepoName, UserId, "Bug");
            await Repository.SetSubscription(RepoOwner, RepoName + "2", UserId, "Bug");
            await Repository.SetSubscription(RepoOwner + "2", RepoName, UserId, "Bug");
        }

        [Fact]
        public async Task QueryUserSubscriptionsForDifferentRepos_DataInDifferentRepos_DataAreIsolated()
        {
            await Repository.SetSubscription(RepoOwner, RepoName, UserId, "Bug");
            await Repository.SetSubscription(RepoOwner, RepoName + "2", UserId, "Bug");
            await Repository.SetSubscription(RepoOwner + "2", RepoName, UserId, "Bug");

            // act
            IEnumerable<LabelSubscriptionModel> userLabelsA = await Repository.GetUserSubscriptions(RepoOwner, RepoName, UserId);
            IEnumerable<LabelSubscriptionModel> userLabelsB = await Repository.GetUserSubscriptions(RepoOwner, RepoName + "2", UserId);
            IEnumerable<LabelSubscriptionModel> userLabelsC = await Repository.GetUserSubscriptions(RepoOwner + "2", RepoName, UserId);

            userLabelsA.Should().HaveCount(1).And.Contain(m => m.RepositoryOwner == RepoOwner && m.RepositoryName == RepoName);
            userLabelsB.Should().HaveCount(1).And.Contain(m => m.RepositoryOwner == RepoOwner && m.RepositoryName == RepoName + "2");
            userLabelsC.Should().HaveCount(1).And.Contain(m => m.RepositoryOwner == RepoOwner + "2" && m.RepositoryName == RepoName);
        }

        [Fact]
        public async Task QueryLabelSubscriptionsForDifferentRepos_DataInDifferentRepos_DataAreIsolated()
        {
            await Repository.SetSubscription(RepoOwner, RepoName, UserId, "Bug");
            await Repository.SetSubscription(RepoOwner, RepoName + "2", UserId, "Bug");
            await Repository.SetSubscription(RepoOwner + "2", RepoName, UserId, "Bug");

            // act
            IEnumerable<LabelSubscriptionModel> userLabelsA = await Repository.GetLabelSubscriptions(RepoOwner, RepoName, "Bug");
            IEnumerable<LabelSubscriptionModel> userLabelsB = await Repository.GetLabelSubscriptions(RepoOwner, RepoName + "2", "Bug");
            IEnumerable<LabelSubscriptionModel> userLabelsC = await Repository.GetLabelSubscriptions(RepoOwner + "2", RepoName, "Bug");

            userLabelsA.Should().HaveCount(1).And.Contain(m => m.RepositoryOwner == RepoOwner && m.RepositoryName == RepoName);
            userLabelsB.Should().HaveCount(1).And.Contain(m => m.RepositoryOwner == RepoOwner && m.RepositoryName == RepoName + "2");
            userLabelsC.Should().HaveCount(1).And.Contain(m => m.RepositoryOwner == RepoOwner + "2" && m.RepositoryName == RepoName);
        }
    }
}
