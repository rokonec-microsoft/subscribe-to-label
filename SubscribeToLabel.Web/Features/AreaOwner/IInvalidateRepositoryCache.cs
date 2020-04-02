namespace DotNet.SubscribeToLabel.Web.Features.AreaOwner
{
    public interface IInvalidateRepositoryCache
    {
        void InvalidateCache(string repositoryOwner, string repositoryName);
    }
}
