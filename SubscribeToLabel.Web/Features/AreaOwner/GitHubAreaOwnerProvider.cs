using DotNet.SubscribeToLabel.Web.Features.GitHubApi;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNet.SubscribeToLabel.Web.Features.AreaOwner
{
    public class GitHubAreaOwnerProvider : IAreaOwnerProvider, IInvalidateRepositoryCache
    {
        private const string AreaOwnersFile = ".github/AREAOWNERS.json";

        private TimeSpan CacheTtl = TimeSpan.FromSeconds(30);
        private readonly ConcurrentDictionary<(string repositoryOwner, string repositoryName), Lazy<Task<AreaOwnerModel>>> _cache 
            = new ConcurrentDictionary<(string repositoryOwner, string repositoryName), Lazy<Task<AreaOwnerModel>>>();

        private readonly IGitHubClientFactory _gitHubClientFactory;

        public GitHubAreaOwnerProvider(IGitHubClientFactory gitHubClientFactory)
        {
            _gitHubClientFactory = gitHubClientFactory;
        }

        public async Task<AreaOwnerModel> Get(string repositoryOwner, string repositoryName)
        {
            if (!_cache.TryGetValue((repositoryOwner, repositoryName), out var areaOwner) || 
                (DateTime.UtcNow - (await areaOwner.Value).RefreshedAt) > CacheTtl)
            {
                areaOwner = _cache.AddOrUpdate((repositoryOwner, repositoryName), 
                    addValueFactory: (repoId) => new Lazy<Task<AreaOwnerModel>>(() => ReadAreaOwnerFile(repoId.repositoryOwner, repoId.repositoryName, null)),
                    updateValueFactory: (repoId, current) => new Lazy<Task<AreaOwnerModel>>(() => ReadAreaOwnerFile(repoId.repositoryOwner, repoId.repositoryName, current))
                );
            }

            return await areaOwner.Value;
        }

        private async Task<AreaOwnerModel> ReadAreaOwnerFile(string repositoryOwner, string repositoryName, Lazy<Task<AreaOwnerModel>>? currentAreaOwner)
        {
            var gitHubClient = await _gitHubClientFactory.GetGitHubInstallationClient();

            var current = currentAreaOwner == null ? null : await currentAreaOwner.Value;

            var commits = await gitHubClient.Repository.Commit.GetAll(repositoryOwner, repositoryName,
                new CommitRequest
                {
                    Path = AreaOwnersFile,
                },
                new ApiOptions
                {
                    StartPage = 1,
                    PageSize = 1,
                    PageCount = 1,
                }
            );

            if (!commits.Any() && current != null || commits.Any() && current != null && current.CommitSha == commits.First().Sha)
            {
                // just update refreshed at
                return new AreaOwnerModel(current.Subscriptions, current.CommitSha, current.CommitDate, 
                    refreshedAt: DateTime.UtcNow);
            }
            else if (!commits.Any())
            {
                return new AreaOwnerModel(new AreaOwnerItem[0], string.Empty, default, refreshedAt: DateTime.UtcNow);
            }

            var commit = commits.First();

            var areaOwnerFile = await gitHubClient.Repository.Content.GetAllContentsByRef(repositoryOwner, repositoryName, AreaOwnersFile, commits.First().Sha);
            var subscriptions = JsonConvert.DeserializeObject<List<AreaOwnerItem>>(areaOwnerFile.First().Content);

            return new AreaOwnerModel(subscriptions, commit.Sha, commit.Commit.Committer.Date, refreshedAt: DateTime.UtcNow);
        }

        public void InvalidateCache(string repositoryOwner, string repositoryName)
        {
            _cache.TryRemove((repositoryOwner, repositoryName), out var areaOwner);
        }
    }
}
