using System;
using System.Threading.Tasks;

namespace DotNet.SubscribeToLabel.Web.Features.IssueSubscriptions
{
    public interface IIssueLabelRepository
    {
        Task<bool> TestLabel(IssueReference issue, string label);
        Task<bool> TrySetLabel(IssueReference issue, string label, bool labeled, DateTime changedAt);
    }
}