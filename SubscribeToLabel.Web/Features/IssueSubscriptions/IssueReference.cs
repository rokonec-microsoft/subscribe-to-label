namespace DotNet.SubscribeToLabel.Web.Features.IssueSubscriptions
{
    public class IssueReference
    {
        public IssueReference(string repositoryOwner, string repositoryName, int number, int id, string nodeId)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            Number = number;
            Id = id;
            NodeId = nodeId;
        }

        /// <summary>
        /// The internal Id for this issue (not the issue number)
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        /// GraphQL Node Id
        /// </summary>
        public string NodeId { get; protected set; }

        /// <summary>
        /// The issue number.
        /// </summary>
        public int Number { get; protected set; }

        /// <summary>
        /// The account's login of repository owner.
        /// </summary>
        public string RepositoryOwner { get; protected set; }

        /// <summary>
        /// The account's login of repository owner.
        /// </summary>
        public string RepositoryName { get; protected set; }

        internal bool HasDifferentMeaningThen(IssueReference other)
        {
            return Number != other.Number || RepositoryOwner != other.RepositoryOwner || RepositoryName != other.RepositoryName;
        }
    }
}