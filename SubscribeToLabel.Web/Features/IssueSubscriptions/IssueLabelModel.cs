using System;

namespace DotNet.SubscribeToLabel.Web.Features.IssueSubscriptions
{
    public class IssueLabelModel
    {
        public IssueLabelModel(IssueReference issue, string label, bool labeled, DateTime lastChanged)
        {
            Issue = issue;
            Label = label;
            Labeled = labeled;
            LastChanged = lastChanged;
        }

        public IssueReference Issue { get; }
        public string Label { get; }
        public bool Labeled { get; }
        public DateTime LastChanged { get; }

        internal bool HasDifferentMeaningThen(IssueLabelModel other)
        {
            return Issue.HasDifferentMeaningThen(other.Issue) || Label != other.Label || Labeled != other.Labeled;
        }
    }
}
