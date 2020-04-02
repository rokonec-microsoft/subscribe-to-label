using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNet.SubscribeToLabel.Web.Features.LabelSubscriptions
{
    public class InMemoryLabelSubscriptionRepository : ILabelSubscriptionRepository
    {
        private readonly ConcurrentDictionary<(string repositoryOwner, string repositoryName, string userId, string label), LabelSubscriptionModel> _store =
            new ConcurrentDictionary<(string repositoryOwner, string repositoryName, string userId, string label), LabelSubscriptionModel>();

        public Task<LabelSubscriptionModel?> GetSubscription(string repositoryOwner, string repositoryName, string userId, string label)
        {
            _store.TryGetValue((repositoryOwner, repositoryName, userId, label), out LabelSubscriptionModel? existing);

            return Task.FromResult(existing);
        }

        public Task<LabelSubscriptionModel> SetSubscription(string repositoryOwner, string repositoryName, string userId, string label)
        {
            var userLabelSubscription = _store.GetOrAdd((repositoryOwner, repositoryName, userId, label), (_) => new LabelSubscriptionModel(repositoryOwner, repositoryName, userId, label));

            return Task.FromResult(userLabelSubscription);
        }

        public Task<IReadOnlyCollection<LabelSubscriptionModel>> GetUserSubscriptions(string repositoryOwner, string repositoryName, string userId)
        {
            IReadOnlyCollection<LabelSubscriptionModel> result = _store
                .Where(o => o.Value.UserId == userId && 
                            o.Value.RepositoryOwner == repositoryOwner &&
                            o.Value.RepositoryName == repositoryName)
                .Select(o => o.Value).ToList();

            return Task.FromResult(result);
        }

        public Task<IReadOnlyCollection<LabelSubscriptionModel>> GetLabelSubscriptions(string repositoryOwner, string repositoryName, string label)
        {
            IReadOnlyCollection<LabelSubscriptionModel> result = _store
                .Where(o =>
                    o.Value.Label == label &&
                    o.Value.RepositoryOwner == repositoryOwner && 
                    o.Value.RepositoryName == repositoryName)
                .Select(o => o.Value).ToList();

            return Task.FromResult(result);
        }

        public Task<bool> DeleteSubscription(string repositoryOwner, string repositoryName, string userId, string label)
        {
            bool result = _store.TryRemove((repositoryOwner, repositoryName, userId, label), out _);

            return Task.FromResult(result);
        }
    }
}