using System;
using System.Text.Json;
using DotNet.SubscribeToLabel.Web.Features.GitHubApi;
using DotNet.SubscribeToLabel.Web.Features.IssueSubscriptions;
using DotNet.SubscribeToLabel.Web.Models.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Octokit;

namespace GitHubAppDotnetSample.Controllers
{
    // this class is taken from the webhook sample in https://github.com/aspnet/AspLabs
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA1801:Remove unused parameter", Justification = "MVC Controller", Scope = "type")]
    public class GitHubWebHookController : ControllerBase
    {
        private readonly IGitHubClientFactory _gitHubClientFactory;
        private readonly IIssueSubscription _issueSubscription;
        private readonly IOptions<GitHubAppOptions> _options;

        public GitHubWebHookController(IGitHubClientFactory gitHubClientFactory, IIssueSubscription issueSubscription, IOptions<GitHubAppOptions> options)
        {
            _gitHubClientFactory = gitHubClientFactory;
            _issueSubscription = issueSubscription;
            _options = options;
        }

        [GitHubWebHook(EventName = "push", Id = "It")]
        public IActionResult HandlerForItsPushes(string[] events, JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok();
        }

        [GitHubWebHook(Id = "It")]
        public IActionResult HandlerForIt(string[] events, JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok();
        }

        [GitHubWebHook(EventName = "issues")]
        public async System.Threading.Tasks.Task<IActionResult> HandlerForPushAsync(string? id, JsonElement data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Octokit.Internal.IJsonSerializer _jsonSerializer = new Octokit.Internal.SimpleJsonSerializer();
            var issueEvent = _jsonSerializer.Deserialize<IssueEventPayload>(data.ToString());

            // get the values from the payload data
            var issueNumber = issueEvent.Issue.Number;
            var installationId = (int)issueEvent.Installation.Id;
            var owner = issueEvent.Repository.Owner.Login;
            var repo = issueEvent.Repository;

            if (installationId != _options.Value.InstallationId)
                return BadRequest("Invalid github webhook installation id");

            IGitHubClient installationClient = await _gitHubClientFactory.GetGitHubInstallationClient();

            if (issueEvent.Action != "labeled")
                return Ok();

            var label = data.GetProperty("label").GetString("name");

            await _issueSubscription.TryAddLabel(new IssueReference(repo.Owner.Login, repo.Name, issueNumber, issueEvent.Issue.Id, issueEvent.Issue.NodeId), label, DateTime.UtcNow);

            return Ok();
        }

        [GitHubWebHook]
        public IActionResult GitHubHandler(string id, string @event, JObject data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok();
        }

        [GeneralWebHook]
        public IActionResult FallbackHandler(string receiverName, string id, string eventName)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok();
        }
    }
}