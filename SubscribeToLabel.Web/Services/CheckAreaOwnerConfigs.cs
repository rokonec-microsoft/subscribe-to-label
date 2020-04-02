using DotNet.SubscribeToLabel.Web.Features.AreaOwner;
using DotNet.SubscribeToLabel.Web.Features.GitHubApi;
using DotNet.SubscribeToLabel.Web.Features.LabelSubscriptions;
using DotNet.SubscribeToLabel.Web.Features.Repositories;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotNet.SubscribeToLabel.Web.Services
{
    public class CheckAreaOwnerConfigs : BackgroundService
    {
        private readonly IGitHubClientFactory _gitHubClientFactory;
        private readonly IListRepositories _listRepositories;
        private readonly IAreaOwnerProvider _areaOwnerProvider;
        private readonly ILabelSubscription _labelSubscription;

        public CheckAreaOwnerConfigs(IGitHubClientFactory gitHubClientFactory, IListRepositories listRepositories, IAreaOwnerProvider areaOwnerProvider, ILabelSubscription labelSubscription)
        {
            _gitHubClientFactory = gitHubClientFactory;
            _listRepositories = listRepositories;
            _areaOwnerProvider = areaOwnerProvider;
            _labelSubscription = labelSubscription;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // read all current for all repos

            var currentlyUsedAreaOwnerConfigs = new Dictionary<(string repositoryOwner, string repositoryName), AreaOwnerModel>();

            var repos = await _listRepositories.GetAllRepositories();
            foreach(var repo in repos)
            {
                // todo: we shall actually initialize with last processed sha which shall be persisted after processing
                //currentlyUsedAreaOwnerConfigs[(repo.Owner, repo.Name)] = await _areaOwnerProvider.Get(repo.Owner, repo.Name);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("test");

                var gitHubClient = await _gitHubClientFactory.GetGitHubInstallationClient();

                repos = await _listRepositories.GetAllRepositories();
                foreach (var repo in repos)
                {
                    if (!currentlyUsedAreaOwnerConfigs.TryGetValue((repo.Owner, repo.Name), out var current))
                    {
                        current = new AreaOwnerModel(new AreaOwnerItem[0], string.Empty, default, default);
                    }

                    var head = await _areaOwnerProvider.Get(repo.Owner, repo.Name);

                    if (head.CommitSha != current.CommitSha)
                    {
                        var h = head.Subscriptions.Select(s => (s.Label, s.User)).ToList();
                        var c = current.Subscriptions.Select(s => (s.Label, s.User)).ToList();

                        var added = h.Except(c).ToList();
                        var deleted = c.Except(h).ToList();

                        foreach (var addition in added)
                        {
                            await _labelSubscription.SetUserSubscription(repo.Owner, repo.Name, addition.User, addition.Label);
                        }
                        foreach (var removal in deleted)
                        {
                            await _labelSubscription.DeleteUserSubscription(repo.Owner, repo.Name, removal.User, removal.Label);
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("start");

            await ExecuteAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("stop");

            return Task.CompletedTask;
        }
    }
}
