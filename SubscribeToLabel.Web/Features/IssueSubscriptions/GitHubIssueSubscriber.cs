using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotNet.SubscribeToLabel.Web.Features.GitHubApi;
using DotNet.SubscribeToLabel.Web.Features.LabelSubscriptions;

namespace DotNet.SubscribeToLabel.Web.Features.IssueSubscriptions
{
    public class GitHubIssueSubscriber : IIssueSubscriber
    {
        private const string CommentMark = "Label notification :bell:";
        private readonly IGitHubClientFactory _gitHubClientFactory;

        public GitHubIssueSubscriber(IGitHubClientFactory gitHubClientFactory)
        {
            _gitHubClientFactory = gitHubClientFactory;
        }

        public async Task SubscribeToIssue(IssueReference issue, IReadOnlyCollection<LabelSubscriptionModel> labelSubscriptions)
        {
            var subscriptions = labelSubscriptions.GroupBy(ls => ls.Label, ls => ls.UserId).ToDictionary(g => g.Key, g => g.ToHashSet());

            var gitHubClient = await _gitHubClientFactory.GetGitHubInstallationClient();

            var comments = await gitHubClient.Issue.Comment.GetAllForIssue(issue.RepositoryOwner, issue.RepositoryName, issue.Number);
            var notifyComment = comments.FirstOrDefault(c => c.Body.Contains(CommentMark, StringComparison.InvariantCultureIgnoreCase));

            if (notifyComment == null)
            {
                var comment = FormatNotifyComment(subscriptions);
                await gitHubClient.Issue.Comment.Create(issue.RepositoryOwner, issue.RepositoryName, issue.Number, comment);
            }
            else
            {
                // syntax: |**label-name**|owner(s)|
                Regex labelSubscriptionPattern = new Regex(@"^\|\s*(?'label'[^,|]+)\s*\|(\s*@(?'users'[\w-/]+))+\s*\|", RegexOptions.Singleline);
                using (StringReader sr = new StringReader(notifyComment.Body))
                {
                    string? line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        var match = labelSubscriptionPattern.Match(line);
                        if (match.Success)
                        {
                            var label = match.Groups["label"].Captures[0].Value;

                            if (!subscriptions.TryGetValue(label, out var subscribers))
                            {
                                subscribers = new HashSet<string>();
                                subscriptions[label] = subscribers;
                            }

                            foreach (Capture? capture in match.Groups["users"].Captures)
                            {
                                if (capture == null)
                                    continue;

                                subscribers.Add(capture.Value);
                            }
                        }
                    }
                }

                var comment = FormatNotifyComment(subscriptions);
                await gitHubClient.Issue.Comment.Update(issue.RepositoryOwner, issue.RepositoryName, notifyComment.Id, comment);
            }
        }

        private static string FormatNotifyComment(Dictionary<string, HashSet<string>> subscriptions)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"### {CommentMark}");
            sb.AppendLine("|label|owner(s)|");
            sb.AppendLine("|-|-|");

            foreach (var pair in subscriptions.OrderBy(kv => kv.Key))
            {
                sb.AppendLine($"|{pair.Key}|{string.Join(' ', pair.Value.OrderBy(s => s).Select(s => "@" + s))}|");
            }

            return sb.ToString();
        }

        public Task UnsubscribeFromIssue(IssueReference issue, IReadOnlyCollection<LabelSubscriptionModel> labelSubscriptions)
        {
            // we do not support unsubscribe as it is not possible to do so by issue comments
            return Task.CompletedTask;
        }
    }
}