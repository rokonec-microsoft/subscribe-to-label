namespace DotNet.SubscribeToLabel.Web.Features.Repositories
{
    public class RepositoryModel
    {
        public RepositoryModel(long id, string owner, string name)
        {
            Owner = owner;
            Name = name;
            Id = id;
        }

        public string Owner { get; }
        public string Name { get; }
        public long Id { get; }
    }
}