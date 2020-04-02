using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DotNet.SubscribeToLabel.Web.Features.IssueSubscriptions
{
    public class InMemoryIssueLabelRepository : IIssueLabelRepository
    {
        private readonly ConcurrentDictionary<(string repoOwner, string repoName, int issueNumber, string label), IssueLabelModel> _store 
            = new ConcurrentDictionary<(string repoOwner, string repoName, int issueNumber, string label), IssueLabelModel>(); 

        public Task<bool> TestLabel(IssueReference issue, string label)
        {
            return Task.FromResult(
                _store.TryGetValue(StoreKey(issue, label), out var existing) && existing.Labeled
            );
        }

        private static (string RepositoryOwner, string RepositoryName, int Number, string label) StoreKey(IssueReference issue, string label)
        {
            return (issue.RepositoryOwner, issue.RepositoryName, issue.Number, label);
        }

        public Task<bool> TrySetLabel(IssueReference issue, string label, bool labeled, DateTime changedAt)
        {
            bool changed = true;

            _store.AddOrUpdate(StoreKey(issue, label),
                addValueFactory: _ => new IssueLabelModel(issue, label, labeled, changedAt),
                updateValueFactory: (_, existing) =>
                {
                    if (IsOutdated(changedAt, existing))
                    {
                        changed = false;
                        return existing;
                    }

                    var toBeUpdated = new IssueLabelModel(issue, label, labeled, changedAt);

                    // if only date time changed we still have to save it to deal with message idempotency and ordering
                    // but for other logic the state of the issue has not changed
                    changed = toBeUpdated.HasDifferentMeaningThen(existing);

                    return toBeUpdated;
                });

            return Task.FromResult(changed);
        }

        private static bool IsOutdated(DateTime changedAt, IssueLabelModel existing) => existing.LastChanged >= changedAt;
    }
}