using System;
using System.Threading.Tasks;

namespace DotNet.SubscribeToLabel.Web.Features.IssueSubscriptions
{
    public interface IIssueSubscription
    {
        Task<bool> TestLabel(IssueReference issue, string label);
        Task<bool> TryAddLabel(IssueReference issue, string label, DateTime at);
        Task<bool> TryRemoveLabel(IssueReference issue, string label, DateTime at);
    }
}