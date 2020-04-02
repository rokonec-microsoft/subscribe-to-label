namespace DotNet.SubscribeToLabel.Web.Features.LabelSubscriptions
{
    public class LabelSubscriptionModel
    {
        public LabelSubscriptionModel(string repositoryOwner, string repositoryName, string userId, string label)
        {
            RepositoryOwner = repositoryOwner;
            RepositoryName = repositoryName;
            UserId = userId;
            Label = label;
        }

        public string RepositoryOwner { get; }
        public string RepositoryName { get; }
        public string UserId { get; }
        public string Label { get; }
    }
}