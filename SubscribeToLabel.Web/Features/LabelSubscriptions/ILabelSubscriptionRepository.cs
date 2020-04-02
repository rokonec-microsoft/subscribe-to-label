using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNet.SubscribeToLabel.Web.Features.LabelSubscriptions
{
    public interface ILabelSubscriptionRepository
    {
        Task<LabelSubscriptionModel?> GetSubscription(string repositoryOwner, string repositoryName, string userId, string label);
        Task<LabelSubscriptionModel> SetSubscription(string repositoryOwner, string repositoryName, string userId, string label);
        Task<IReadOnlyCollection<LabelSubscriptionModel>> GetUserSubscriptions(string repositoryOwner, string repositoryName, string userId);
        Task<IReadOnlyCollection<LabelSubscriptionModel>> GetLabelSubscriptions(string repositoryOwner, string repositoryName, string label);
        Task<bool> DeleteSubscription(string repositoryOwner, string repositoryName, string userId, string label);
    }
}