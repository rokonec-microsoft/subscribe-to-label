using System;
using System.Threading.Tasks;
using DotNet.SubscribeToLabel.Web.Features.LabelSubscriptions;
using DotNet.SubscribeToLabel.Web.Features.Shared;

namespace DotNet.SubscribeToLabel.Web.Features.IssueSubscriptions
{
    public class IssueSubscription : IIssueSubscription
    {
        private readonly IIssueLabelRepository _issueLabelRepository;
        private readonly ILabelSubscriptionRepository _userLabelRepository;
        private readonly IIssueSubscriber _issueSubscriber;

        public IssueSubscription(IIssueLabelRepository issueLabelRepository, ILabelSubscriptionRepository userLabelRepository, IIssueSubscriber issueSubscriber)
        {
            _issueLabelRepository = issueLabelRepository;
            _userLabelRepository = userLabelRepository;
            _issueSubscriber = issueSubscriber;
        }

        public async Task<bool> TryAddLabel(IssueReference issue, string label, DateTime at)
        {
            Validation.ValidateIssue(issue);
            Validation.ValidateLabel(label);

            bool added = await _issueLabelRepository.TrySetLabel(issue, label, true, at);

            if (added)
            {
                var labelSubscriptions = await _userLabelRepository.GetLabelSubscriptions(issue.RepositoryOwner, issue.RepositoryName, label);
                if (labelSubscriptions.Count > 0)
                {
                    await _issueSubscriber.SubscribeToIssue(issue, labelSubscriptions);
                }
            }

            return added;
        }

        public async Task<bool> TryRemoveLabel(IssueReference issue, string label, DateTime at)
        {
            Validation.ValidateIssue(issue);
            Validation.ValidateLabel(label);

            bool removed = await _issueLabelRepository.TrySetLabel(issue, label, false, at);

            if (removed)
            {
                var labelSubscriptions = await _userLabelRepository.GetLabelSubscriptions(issue.RepositoryOwner, issue.RepositoryName, label);
                if (labelSubscriptions.Count > 0)
                {
                    await _issueSubscriber.UnsubscribeFromIssue(issue, labelSubscriptions);
                }
            }

            return removed;
        }

        public Task<bool> TestLabel(IssueReference issue, string label)
        {
            Validation.ValidateIssue(issue);
            Validation.ValidateLabel(label);

            return _issueLabelRepository.TestLabel(issue, label);
        }
    }
}
