using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet.SubscribeToLabel.Web.Features.LabelSubscriptions;

namespace DotNet.SubscribeToLabel.Web.Features.IssueSubscriptions
{
    public interface IIssueSubscriber
    {
        Task SubscribeToIssue(IssueReference issue, IReadOnlyCollection<LabelSubscriptionModel> labelSubscriptions);
        Task UnsubscribeFromIssue(IssueReference issue, IReadOnlyCollection<LabelSubscriptionModel> labelSubscriptions);
    }
}