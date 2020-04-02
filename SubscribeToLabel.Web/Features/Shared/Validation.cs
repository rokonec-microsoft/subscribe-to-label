using System;
using DotNet.SubscribeToLabel.Web.Features.IssueSubscriptions;

namespace DotNet.SubscribeToLabel.Web.Features.Shared
{
    public static class Validation
    {
        public static void ValidateLabel(string label)
        {
            if (label is null)
                throw new ArgumentNullException(nameof(label));

            if (label == string.Empty)
                throw new ArgumentException("label is empty", nameof(label));

            if (label.Contains(",", StringComparison.Ordinal))
                throw new ArgumentException("label contains ','", nameof(label));

            if (label.Contains("|", StringComparison.Ordinal))
                throw new ArgumentException("label contains '|'", nameof(label));
        }

        public static void ValidateRepository(string repositoryOwner, string repositoryName)
        {
            if (repositoryOwner is null) throw new ArgumentNullException(nameof(repositoryOwner));
            if (repositoryOwner == string.Empty) throw new ArgumentException("repositoryOwner is empty", nameof(repositoryOwner));
            if (repositoryName is null) throw new ArgumentNullException(nameof(repositoryName));
            if (repositoryName == string.Empty) throw new ArgumentException("repositoryName is empty", nameof(repositoryName));
        }

        public static void ValidateIssue(IssueReference issue)
        {
            if (issue is null)
                throw new ArgumentNullException(nameof(issue));

            if (issue.Id <= 0)
                throw new ArgumentOutOfRangeException(nameof(issue.Id), issue.Id, "Issue Id has to be positive non zero number.");

            if (issue.Number <= 0)
                throw new ArgumentOutOfRangeException(nameof(issue.Number), issue.Number, "Issue number has to be positive non zero number.");

            if (string.IsNullOrWhiteSpace(issue.RepositoryOwner))
                throw new ArgumentException("Repository owner has to be non empty.", nameof(issue.RepositoryOwner));

            if (string.IsNullOrWhiteSpace(issue.RepositoryName))
                throw new ArgumentException("Repository name has to be non empty.", nameof(issue.RepositoryName));
        }
    }
}
