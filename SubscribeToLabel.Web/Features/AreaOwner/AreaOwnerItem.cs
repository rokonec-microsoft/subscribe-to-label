using System.Diagnostics;

namespace DotNet.SubscribeToLabel.Web.Features.AreaOwner
{
    [DebuggerDisplay("Label {Label} User = {User}")]
    public class AreaOwnerItem
    {
        public AreaOwnerItem(string label, string user)
        {
            Label = label;
            User = user;
        }

        public string Label { get; private set; }
        public string User { get; private set; }
    }
}
