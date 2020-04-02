using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNet.SubscribeToLabel.Web.Features.IssueSubscriptions;
using DotNet.SubscribeToLabel.Web.Features.Shared;

namespace DotNet.SubscribeToLabel.Web.Features.LabelSubscriptions
{
    public class LabelSubscription : ILabelSubscription
    {
        private readonly ILabelSubscriptionRepository _repository;
        private readonly IIssueSubscriber _issueSubscriber;
        private readonly IIssueQuery _issueQuery;

        public LabelSubscription(ILabelSubscriptionRepository repository, IIssueSubscriber issueSubscriber, IIssueQuery issueQuery)
        {
            _repository = repository;
            _issueSubscriber = issueSubscriber;
            _issueQuery = issueQuery;
        }

        public Task<LabelSubscriptionModel> SetUserSubscription(string repositoryOwner, string repositoryName, string userId, string label)
        {
            return SetUserSubscriptionInternal(repositoryOwner, repositoryName, userId, label);
        }

        private async Task<LabelSubscriptionModel> SetUserSubscriptionInternal(string repositoryOwner, string repositoryName, string userId, string label)
        {
            if (userId is null) throw new ArgumentNullException(nameof(userId));
            Validation.ValidateLabel(label);
            Validation.ValidateRepository(repositoryOwner, repositoryName);

            var labelSubscription = await _repository.SetSubscription(repositoryOwner, repositoryName, userId, label);
            var labelSubscriptions = new [] {labelSubscription};

            foreach (var issue in await _issueQuery.SearchOpenIssuesWithLabel(repositoryOwner, repositoryName, label))
            {
                await _issueSubscriber.SubscribeToIssue(issue, labelSubscriptions);
            }

            return labelSubscription;
        }

        public Task<IReadOnlyCollection<LabelSubscriptionModel>> GetUserSubscriptions(string repositoryOwner, string repositoryName, string userId)
        {
            Validation.ValidateRepository(repositoryOwner, repositoryName);
            return _repository.GetUserSubscriptions(repositoryOwner, repositoryName, userId);
        }

        public async Task<IReadOnlyCollection<LabelSubscriptionModel>> SetUserSubscriptions(string repositoryOwner, string repositoryName, string userId, IReadOnlyCollection<string> labels)
        {
            if (userId is null) throw new ArgumentNullException(nameof(userId));
            if (labels is null) throw new ArgumentNullException(nameof(labels));
            Validation.ValidateRepository(repositoryOwner, repositoryName);
            foreach (var label in labels)
            {
                Validation.ValidateLabel(label);
            }

            var existing = (await _repository.GetUserSubscriptions(repositoryOwner, repositoryName, userId)).Select(o => o.Label).ToList();
            var toBeAdded = labels.Except(existing).ToList();
            var toBeDeleted = existing.Except(labels).ToList();

            foreach (var label in toBeAdded)
            {
                await SetUserSubscriptionInternal(repositoryOwner, repositoryName, userId, label);
            }

            foreach (var label in toBeDeleted)
            {
                await _repository.DeleteSubscription(repositoryOwner, repositoryName, userId, label);
            }

            return await GetUserSubscriptions(repositoryOwner, repositoryName, userId);
        }

        public async Task<bool> DeleteUserSubscription(string repositoryOwner, string repositoryName, string userId, string label)
        {
            return await _repository.DeleteSubscription(repositoryOwner, repositoryName, userId, label);
        }
    }
}
