using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet.SubscribeToLabel.Web.Features.IssueSubscriptions;

namespace DotNet.SubscribeToLabel.Web.Features.LabelSubscriptions
{
    public interface IIssueQuery
    {
        Task<IReadOnlyCollection<IssueReference>> SearchOpenIssuesWithLabel(string repositoryOwner, string repositoryName, string label);
    }
}
