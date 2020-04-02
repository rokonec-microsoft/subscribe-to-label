using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNet.SubscribeToLabel.Web.Features.LabelSubscriptions
{
    public interface ILabelSubscription
    {
        Task<IReadOnlyCollection<LabelSubscriptionModel>> GetUserSubscriptions(string repositoryOwner, string repositoryName, string userId);

        /// <summary>
        /// Compare with labels existing and sync it (add added, delete missing)
        /// </summary>
        Task<IReadOnlyCollection<LabelSubscriptionModel>> SetUserSubscriptions(string repositoryOwner, string repositoryName, string userId, IReadOnlyCollection<string> labels);

        Task<LabelSubscriptionModel> SetUserSubscription(string repositoryOwner, string repositoryName, string userId, string label);

        Task<bool> DeleteUserSubscription(string repositoryOwner, string repositoryName, string userId, string label);
    }
}
