using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DotNet.SubscribeToLabel.Web.Features.AreaOwner
{
    [DebuggerDisplay("CommitSha: {CommitSha}, CommitDate: {CommitDate}, RefreshedAt: {RefreshedAt}")]
    public class AreaOwnerModel
    {
        public AreaOwnerModel(IReadOnlyCollection<AreaOwnerItem> subscriptions, string commitSha, DateTimeOffset commitDate, DateTime refreshedAt)
        {
            Subscriptions = subscriptions;
            CommitSha = commitSha;
            CommitDate = commitDate;
            RefreshedAt = refreshedAt;
        }

        public IReadOnlyCollection<AreaOwnerItem> Subscriptions { get; private set; }
        public string CommitSha { get; private set; }
        public DateTimeOffset CommitDate { get; private set; }
        public DateTime RefreshedAt { get; private set; }
    }
}
